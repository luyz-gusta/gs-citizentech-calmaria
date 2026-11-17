using System.Runtime.CompilerServices;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Varredura;

/// <summary>
/// Um texto lido de um destino (uma linha de log, um comentário gravado, uma mensagem do broker). O texto é cru:
/// o <see cref="ServicoVarredura"/> passa ele pelo motor e só o achado mascarado sai da varredura.
/// </summary>
/// <param name="Local">Onde o texto estava, sem dado pessoal (ex.: calmaria-20261117.log:42, relato 31).</param>
/// <param name="Texto">O texto lido.</param>
public sealed record TrechoVarrido(string Local, string Texto);

/// <summary>Um lugar onde o dado do cidadão pode ter ido parar. Cada fonte é uma classe.</summary>
public interface IFonteVarredura
{
    /// <summary>LOGS, RELATOS, MQTT ou CODIGO.</summary>
    string Nome { get; }

    /// <summary>O destino que esta fonte representa, para o peso da exposição.</summary>
    DestinoFluxo Destino { get; }

    /// <summary>Só estas categorias contam nesta fonte; nulo conta todas. A de código só procura segredo.</summary>
    IReadOnlySet<CategoriaPii>? Categorias => null;

    IAsyncEnumerable<TrechoVarrido> LerAsync(CancellationToken ct = default);
}

/// <summary>A fonte não pôde ser lida (pasta que não existe, broker desligado). A varredura segue sem ela.</summary>
public sealed class FonteIndisponivelException(string motivo) : Exception(motivo);

/// <summary>Configuração das varreduras (seção Privacidade:Varredura).</summary>
public sealed class OpcoesDaVarredura
{
    public const string Secao = "Privacidade:Varredura";

    /// <summary>A pasta dos arquivos de log da API.</summary>
    public string? PastaDeLogs { get; set; }

    /// <summary>A raiz do repositório, para a busca de segredo no código. Vazio: a fonte de código fica de fora.</summary>
    public string? PastaDeCodigo { get; set; }

    /// <summary>Os logs mais recentes que entram, no máximo.</summary>
    public int MaximoDeArquivosDeLog { get; set; } = 10;

    /// <summary>Arquivo maior que isso fica de fora (log gigante, binário).</summary>
    public long TamanhoMaximoDoArquivo { get; set; } = 5 * 1024 * 1024;

    /// <summary>Quanto tempo de fluxos bloqueados entra no Risco Cidadão (7.00:00:00 é uma semana).</summary>
    public TimeSpan JanelaDeFluxos { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Pastas que nunca entram na busca de código, em qualquer nível.</summary>
    public List<string> PastasIgnoradas { get; set; } =
        ["bin", "obj", ".git", ".vs", ".idea", "node_modules", "TestResults", "artefatos", ".expo", "__pycache__", ".venv"];

    /// <summary>
    /// Caminhos a partir da raiz que ficam de fora: os testes e os exemplos têm segredos falsos de propósito, para
    /// testar a regra e mostrar o Semgrep.
    /// </summary>
    public List<string> CaminhosIgnorados { get; set; } = ["api-dotnet/tests", "security/exemplos", "docs"];

    /// <summary>As extensões de código e configuração que a busca de segredo lê.</summary>
    public List<string> Extensoes { get; set; } =
    [
        ".cs", ".json", ".ps1", ".sh", ".yml", ".yaml", ".env", ".config", ".xml", ".props", ".ts", ".tsx", ".js",
        ".py", ".ipynb", ".ino", ".cpp", ".h", ".ini", ".toml", ".properties", ".sql", ".service", ".conf"
    ];
}

/// <summary>Os arquivos de log da API: o dado que um log de depuração gravou cru fica aqui.</summary>
public sealed class FonteLogs(OpcoesDaVarredura opcoes) : IFonteVarredura
{
    public string Nome => "LOGS";
    public DestinoFluxo Destino => DestinoFluxo.Log;

    public async IAsyncEnumerable<TrechoVarrido> LerAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(opcoes.PastaDeLogs) || !Directory.Exists(opcoes.PastaDeLogs))
            throw new FonteIndisponivelException("A pasta de logs não foi configurada ou não existe.");

        var arquivos = new DirectoryInfo(opcoes.PastaDeLogs).EnumerateFiles("*.log")
            .Where(f => f.Length <= opcoes.TamanhoMaximoDoArquivo)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Take(opcoes.MaximoDeArquivosDeLog);

        foreach (var arquivo in arquivos)
        {
            // O log do dia está aberto pela própria API: ler sem travar quem escreve.
            await using var fluxo = new FileStream(arquivo.FullName, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var leitor = new StreamReader(fluxo);
            var numero = 0;
            while (await leitor.ReadLineAsync(ct) is { } linha)
            {
                numero++;
                yield return new TrechoVarrido($"{arquivo.Name}:{numero}", linha);
            }
        }
    }
}

/// <summary>
/// O código e a configuração do repositório, atrás de segredo (senha em string de conexão, chave de API, token).
/// Um segredo no repositório vaza para quem recebe o ZIP ou clona o projeto.
/// </summary>
public sealed class FonteCodigo(OpcoesDaVarredura opcoes) : IFonteVarredura
{
    private static readonly IReadOnlySet<CategoriaPii> SoSegredo = new HashSet<CategoriaPii> { CategoriaPii.Segredo };

    public string Nome => "CODIGO";
    public DestinoFluxo Destino => DestinoFluxo.Arquivo;
    public IReadOnlySet<CategoriaPii>? Categorias => SoSegredo;

    public async IAsyncEnumerable<TrechoVarrido> LerAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(opcoes.PastaDeCodigo) || !Directory.Exists(opcoes.PastaDeCodigo))
            throw new FonteIndisponivelException("A pasta do código não foi configurada ou não existe.");

        var raiz = Path.GetFullPath(opcoes.PastaDeCodigo);
        foreach (var arquivo in ArquivosDe(raiz))
        {
            var relativo = Path.GetRelativePath(raiz, arquivo).Replace('\\', '/');
            var numero = 0;
            foreach (var linha in await File.ReadAllLinesAsync(arquivo, ct))
            {
                numero++;
                yield return new TrechoVarrido($"{relativo}:{numero}", linha);
            }
        }
    }

    private IEnumerable<string> ArquivosDe(string raiz)
    {
        var extensoes = new HashSet<string>(opcoes.Extensoes, StringComparer.OrdinalIgnoreCase);
        var pastasIgnoradas = new HashSet<string>(opcoes.PastasIgnoradas, StringComparer.OrdinalIgnoreCase);
        var pendentes = new Stack<string>();
        pendentes.Push(raiz);

        while (pendentes.Count > 0)
        {
            var pasta = pendentes.Pop();
            foreach (var sub in Directory.EnumerateDirectories(pasta))
            {
                var relativo = Path.GetRelativePath(raiz, sub).Replace('\\', '/');
                if (!pastasIgnoradas.Contains(Path.GetFileName(sub)) && !Ignorado(relativo))
                    pendentes.Push(sub);
            }

            foreach (var arquivo in Directory.EnumerateFiles(pasta).Order(StringComparer.Ordinal))
            {
                var info = new FileInfo(arquivo);
                if (extensoes.Contains(info.Extension) && info.Length <= opcoes.TamanhoMaximoDoArquivo)
                    yield return arquivo;
            }
        }
    }

    private bool Ignorado(string relativo) =>
        opcoes.CaminhosIgnorados.Any(c => relativo.Equals(c, StringComparison.OrdinalIgnoreCase)
                                          || relativo.StartsWith(c.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase));
}
