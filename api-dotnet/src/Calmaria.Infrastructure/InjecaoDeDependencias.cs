using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Comunidade;
using Calmaria.Infrastructure.Mqtt;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Sensorial;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;
using Calmaria.Privacidade.Taint;
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

    /// <summary>Lugares, leituras, índice, horários calmos, refúgio e o consumidor MQTT.</summary>
    public static IServiceCollection AddModuloSensorial(this IServiceCollection services, IConfiguration configuracao)
    {
        var opcoesIndice = configuracao.GetSection(IndiceOptions.Secao).Get<IndiceOptions>() ?? new IndiceOptions();
        services.AddSingleton(opcoesIndice);
        services.AddSingleton<ICalculadoraIndice, CalculadoraIndicePonderada>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<LugarService>();
        services.AddScoped<ILugarService>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILugarConsulta>(sp => sp.GetRequiredService<LugarService>());
        services.AddScoped<ILeituraService, LeituraService>();
        services.AddScoped<IHorarioCalmoService, HorarioCalmoService>();
        services.AddScoped<IRefugioService, RefugioService>();

        var secaoMqtt = configuracao.GetSection(MqttOptions.Secao);
        services.Configure<MqttOptions>(secaoMqtt);
        services.AddSingleton<EstadoMqtt>();
        services.AddHealthChecks().AddCheck<MqttHealthCheck>("mqtt");
        if (secaoMqtt.GetValue<bool>(nameof(MqttOptions.Habilitado)))
            services.AddHostedService<LeiturasMqttWorker>();

        services.AddScoped<IPrevisaoService, PrevisaoService>();
        return services;
    }

    /// <summary>Usuário anônimo, perfil sensorial e relatos. Precisa do Sensorial (lugares) e da Privacidade.</summary>
    public static IServiceCollection AddModuloComunidade(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IRelatoService, RelatoService>();
        return services;
    }

    /// <summary>
    /// O motor de PII com as regras fixas e as do regras-pii.json, e o Sanitizador. O caminho do arquivo pode ser
    /// trocado em Privacidade:ArquivoDeRegras.
    /// </summary>
    public static IServiceCollection AddModuloPrivacidade(this IServiceCollection services, IConfiguration configuracao)
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

        services.AddSingleton<IMotorPii, MotorPii>();
        services.AddSingleton<ISanitizador, Sanitizador>();
        return services;
    }
}
