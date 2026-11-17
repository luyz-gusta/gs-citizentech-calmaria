using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Api.Privacidade;

/// <summary>
/// Guarda de saída do log (taint analysis em tempo de execução). Embrulha um provedor de log: toda mensagem passa
/// pelo motor de PII antes de ser escrita. Com dado pessoal, a linha sai mascarada, igual ao Sanitizador
/// ([removido], &lt;SAUDE&gt;), e, se houver <paramref name="rastreador"/>, o fluxo bloqueado é registrado. Pega o
/// que o sistema de tipos não vê, como o corpo da requisição gravado por um log de depuração.
/// </summary>
/// <remarks>
/// Cada provedor (console, arquivo, depuração) recebe a mesma linha. Por isso, em <see cref="ProtecaoDeLog"/>, os
/// provedores embrulhados só mascaram, e um provedor à parte, que não escreve nada, registra o fluxo uma vez só.
/// </remarks>
public sealed class LoggerProviderComRedacao(ILoggerProvider interno, IMotorPii motor, IRastreadorDeFluxo? rastreador)
    : ILoggerProvider, ISupportExternalScope
{
    public const string LinhaOmitida = "[linha de log omitida: o filtro de privacidade passou do tempo limite]";

    /// <summary>O provedor embrulhado (console, arquivo, depuração).</summary>
    public ILoggerProvider Interno => interno;

    public ILogger CreateLogger(string categoryName) =>
        new LoggerComRedacao(interno.CreateLogger(categoryName), categoryName, motor, rastreador);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        if (interno is ISupportExternalScope comEscopo)
            comEscopo.SetScopeProvider(scopeProvider);
    }

    public void Dispose() => interno.Dispose();

    private sealed class LoggerComRedacao(ILogger interno, string categoria, IMotorPii motor, IRastreadorDeFluxo? rastreador)
        : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => interno.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => interno.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!interno.IsEnabled(logLevel))
                return;

            // A exceção entra na análise: mensagem de erro também carrega o dado que chegou na requisição.
            var mensagem = formatter(state, exception);
            var completo = exception is null ? mensagem : $"{mensagem}{Environment.NewLine}{exception}";

            IReadOnlyList<Achado> achados;
            try
            {
                achados = motor.Analisar(completo);
            }
            catch (RegexMatchTimeoutException)
            {
                interno.Log(logLevel, eventId, LinhaOmitida, null, static (s, _) => s);
                return;
            }

            if (achados.Count == 0)
            {
                interno.Log(logLevel, eventId, state, exception, formatter);
                return;
            }

            rastreador?.Registrar($"log:{categoria}", DestinoFluxo.Log, achados, bloqueado: true);
            interno.Log(logLevel, eventId, Sanitizador.Mascarar(completo, achados), null, static (s, _) => s);
        }
    }
}

/// <summary>Liga o guarda em todos os provedores de log registrados.</summary>
public static class ProtecaoDeLog
{
    /// <summary>
    /// Troca cada provedor de log registrado pelo mesmo provedor embrulhado no <see cref="LoggerProviderComRedacao"/>.
    /// Chamar depois de registrar todos os provedores, logo antes do builder.Build(). As regras de nível que valem
    /// são as gerais (Logging:LogLevel); as específicas de um provedor deixam de casar.
    /// </summary>
    public static IServiceCollection ProtegerLogsComRedacao(this IServiceCollection services)
    {
        var originais = services.Where(d => d.ServiceType == typeof(ILoggerProvider) && !d.IsKeyedService).ToList();
        foreach (var original in originais)
        {
            services.Remove(original);

            // O tipo concreto vira serviço dele mesmo, para o contêiner escolher o construtor como sempre faz.
            if (original.ImplementationType is { } tipo)
                services.Add(ServiceDescriptor.Singleton(tipo, tipo));

            services.AddSingleton<ILoggerProvider>(sp => new LoggerProviderComRedacao(Criar(sp, original),
                sp.GetRequiredService<IMotorPii>(), rastreador: null));
        }

        // Um registro de fluxo por linha de log, por mais provedores que existam.
        services.AddSingleton<ILoggerProvider>(sp => new LoggerProviderComRedacao(new ProvedorQueDescarta(),
            sp.GetRequiredService<IMotorPii>(), sp.GetRequiredService<IRastreadorDeFluxo>()));
        return services;
    }

    private static ILoggerProvider Criar(IServiceProvider sp, ServiceDescriptor original) =>
        original.ImplementationInstance as ILoggerProvider
        ?? original.ImplementationFactory?.Invoke(sp) as ILoggerProvider
        ?? (ILoggerProvider)sp.GetRequiredService(original.ImplementationType!);
}

/// <summary>Um provedor que aceita toda linha e não escreve nada: só serve para o guarda registrar o fluxo.</summary>
internal sealed class ProvedorQueDescarta : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Descarta();

    public void Dispose()
    {
    }

    private sealed class Descarta : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
