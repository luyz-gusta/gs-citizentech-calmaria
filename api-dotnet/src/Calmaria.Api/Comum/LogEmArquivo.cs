using Calmaria.Domain.Comum;

namespace Calmaria.Api.Comum;

/// <summary>
/// Grava o log em arquivo, um por dia (calmaria-AAAAMMDD.log, no horário de São Paulo). É o log que a varredura de
/// privacidade confere. Ligado por LogEmArquivo:Pasta; vazio desliga. Passa pelo guarda de redação como todo
/// outro provedor de log.
/// </summary>
public sealed class ProvedorDeLogEmArquivo(string pasta, TimeProvider relogio) : ILoggerProvider
{
    private readonly Lock trava = new();
    private StreamWriter? escritor;
    private DateOnly diaAberto;

    public ILogger CreateLogger(string categoryName) => new LoggerEmArquivo(this, categoryName);

    internal void Escrever(LogLevel nivel, string categoria, string mensagem)
    {
        var agora = HorarioDeSaoPaulo.Converter(relogio.GetUtcNow().UtcDateTime);
        var linha = $"{agora:yyyy-MM-dd HH:mm:ss zzz} [{Sigla(nivel)}] {categoria}: {mensagem}";
        lock (trava)
        {
            var dia = DateOnly.FromDateTime(agora.DateTime);
            if (escritor is null || dia != diaAberto)
            {
                escritor?.Dispose();
                Directory.CreateDirectory(pasta);
                var arquivo = new FileStream(Path.Combine(pasta, $"calmaria-{dia:yyyyMMdd}.log"), FileMode.Append,
                    FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                escritor = new StreamWriter(arquivo) { AutoFlush = true };
                diaAberto = dia;
            }

            escritor.WriteLine(linha);
        }
    }

    public void Dispose()
    {
        lock (trava)
        {
            escritor?.Dispose();
            escritor = null;
        }
    }

    private static string Sigla(LogLevel nivel) => nivel switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "AVS",
        LogLevel.Error => "ERR",
        _ => "CRT"
    };

    private sealed class LoggerEmArquivo(ProvedorDeLogEmArquivo provedor, string categoria) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel is >= LogLevel.Information and < LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var mensagem = formatter(state, exception);
            if (exception is not null)
                mensagem = $"{mensagem}{Environment.NewLine}{exception}";
            provedor.Escrever(logLevel, categoria, mensagem);
        }
    }
}
