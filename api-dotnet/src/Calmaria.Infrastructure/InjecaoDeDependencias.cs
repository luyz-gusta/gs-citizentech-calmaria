using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Mqtt;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Privacidade;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;
using Calmaria.Privacidade.Remediacao;
using Calmaria.Privacidade.Risco;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Calmaria.Infrastructure;

/// <summary>Um método por módulo, para o Program.cs ficar curto e cada módulo se registrar sozinho.</summary>
public static class InjecaoDeDependencias
{
    /// <summary>O banco (PostgreSQL) e a verificação de saúde dele em /health.</summary>
    public static IServiceCollection AddPersistencia(this IServiceCollection services, string conexao)
    {
        services.AddDbContext<CalmariaDbContext>(o => o.UseNpgsql(conexao));
        services.AddHealthChecks().AddDbContextCheck<CalmariaDbContext>("banco");
        return services;
    }

    /// <summary>O índice de sobrecarga e o consumidor MQTT. Os casos de uso do módulo estão em Calmaria.Application.</summary>
    public static IServiceCollection AddModuloSensorial(this IServiceCollection services, IConfiguration configuracao)
    {
        var opcoesIndice = configuracao.GetSection(IndiceOptions.Secao).Get<IndiceOptions>() ?? new IndiceOptions();
        services.AddSingleton(opcoesIndice);
        services.AddSingleton<ICalculadoraIndice, CalculadoraIndicePonderada>();
        services.TryAddSingleton(TimeProvider.System);

        var secaoMqtt = configuracao.GetSection(MqttOptions.Secao);
        services.Configure<MqttOptions>(secaoMqtt);
        services.AddSingleton<EstadoMqtt>();
        services.AddHealthChecks().AddCheck<MqttHealthCheck>("mqtt");
        if (secaoMqtt.GetValue<bool>(nameof(MqttOptions.Habilitado)))
            services.AddHostedService<LeiturasMqttWorker>();
        return services;
    }

    /// <summary>
    /// O módulo Privacidade: o motor de PII com as regras fixas e as do regras-pii.json, o Sanitizador, o
    /// rastreador de fluxo, as fontes de varredura, o tradutor por IA (LLM com frase pronta de reserva) e o Risco
    /// Cidadão. O caminho das regras pode ser trocado em Privacidade:ArquivoDeRegras.
    /// </summary>
    /// <param name="services">Os serviços da API.</param>
    /// <param name="configuracao">A configuração (seções Privacidade e Llm).</param>
    /// <param name="pastaPadraoDeLogs">Onde a API grava o log, se Privacidade:Varredura:PastaDeLogs não disser.</param>
    /// <param name="pastaPadraoDeCodigo">A raiz do repositório, se houver, para a busca de segredo.</param>
    public static IServiceCollection AddModuloPrivacidade(this IServiceCollection services, IConfiguration configuracao,
        string? pastaPadraoDeLogs = null, string? pastaPadraoDeCodigo = null)
    {
        services.AddSingleton<IRegraPii, RegraCpf>();
        services.AddSingleton<IRegraPii, RegraTelefone>();
        services.AddSingleton<IRegraPii, RegraEmail>();
        services.AddSingleton<IRegraPii, RegraSaude>();
        services.AddSingleton<IRegraPii, RegraSegredo>();
        services.AddSingleton<IRegraPii, RegraLocalizacaoPrecisa>();
        services.AddSingleton<IRegraPii, RegraQuaseIdentificador>();

        var arquivo = configuracao["Privacidade:ArquivoDeRegras"]
                      ?? Path.Combine(AppContext.BaseDirectory, "Regras", "regras-pii.json");
        foreach (var regra in RegraConfiguravel.CarregarDe(arquivo))
            services.AddSingleton<IRegraPii>(regra);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IMotorPii, MotorPii>();
        services.AddSingleton<ISanitizador, Sanitizador>();
        services.AddSingleton<IRastreadorDeFluxo, RastreadorDeFluxo>();
        services.AddSingleton<ICalculadoraRiscoCidadao, CalculadoraRiscoCidadao>();

        // Varredura: logs e código em disco, relatos no banco, mensagens do broker em memória.
        var opcoesVarredura = configuracao.GetSection(OpcoesDaVarredura.Secao).Get<OpcoesDaVarredura>()
                              ?? new OpcoesDaVarredura();
        opcoesVarredura.PastaDeLogs ??= pastaPadraoDeLogs;
        opcoesVarredura.PastaDeCodigo ??= pastaPadraoDeCodigo;
        services.AddSingleton(opcoesVarredura);
        services.AddSingleton<IFonteVarredura, FonteLogs>();
        services.AddScoped<IFonteVarredura, FonteRelatos>();
        services.AddSingleton<IFonteVarredura, FonteMqtt>();
        services.AddSingleton<IFonteVarredura, FonteCodigo>();
        services.AddScoped<ServicoVarredura>();

        // O LLM por REST, com HttpClient tipado: tempo limite por tentativa, uma nova tentativa e disjuntor. Se
        // o serviço cair, o disjuntor abre e o tradutor cai direto na frase pronta, sem esperar.
        var opcoesLlm = configuracao.GetSection(OpcoesLlm.Secao).Get<OpcoesLlm>() ?? new OpcoesLlm();
        services.AddSingleton(opcoesLlm);
        services.AddSingleton<TradutorModelo>();
        var tentativa = TimeSpan.FromSeconds(Math.Max(1, opcoesLlm.TempoLimiteEmSegundos));
        services.AddHttpClient<ITradutorDeRisco, TradutorLlm>(cliente =>
            {
                // A chave vai em cada pedido, pelo próprio TradutorLlm.
                if (opcoesLlm.Configurado)
                    cliente.BaseAddress = new Uri(opcoesLlm.UrlBase!.TrimEnd('/') + "/");
            })
            .AddStandardResilienceHandler(o =>
            {
                o.AttemptTimeout.Timeout = tentativa;
                o.TotalRequestTimeout.Timeout = tentativa * 2 + TimeSpan.FromSeconds(5);
                o.Retry.MaxRetryAttempts = 1;
                o.Retry.Delay = TimeSpan.FromSeconds(1);
                o.CircuitBreaker.MinimumThroughput = 2;
                o.CircuitBreaker.FailureRatio = 0.5;
                o.CircuitBreaker.SamplingDuration = tentativa * 2 + TimeSpan.FromSeconds(10);
                o.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            });

        services.AddScoped<IServicoDePrivacidade, ServicoDePrivacidade>();
        return services;
    }
}
