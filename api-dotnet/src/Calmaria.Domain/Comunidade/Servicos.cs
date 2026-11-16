using Calmaria.Privacidade.Taint;

namespace Calmaria.Domain.Comunidade;

/// <summary>Os dados do perfil, como chegam e como saem.</summary>
public sealed record DadosDoPerfil(int SensibilidadeSom, int SensibilidadeLuz, int SensibilidadeMultidao,
    string? ContatoApoioNome, string? ContatoApoioTelefone);

public interface IUsuarioService
{
    Task<Guid> CriarAsync(TextoNaoConfiavel? apelido, CancellationToken ct = default);

    /// <summary>Nulo se o usuário não existe ou ainda não preencheu o perfil.</summary>
    Task<DadosDoPerfil?> ObterPerfilAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Grava o perfil; usuário que não existe: 404.</summary>
    Task SalvarPerfilAsync(Guid usuarioId, DadosDoPerfil dados, CancellationToken ct = default);

    /// <summary>Apaga o usuário, o perfil e os relatos (LGPD, art. 18). Falso se não existia.</summary>
    Task<bool> ExcluirAsync(Guid usuarioId, CancellationToken ct = default);
}

/// <summary>Um relato que chegou do app. O comentário ainda é não confiável.</summary>
public sealed record NovoRelato(Guid UsuarioId, int LugarId, int Nota, IReadOnlyList<Gatilho> Gatilhos,
    TextoNaoConfiavel? Comentario, DateTime VisitadoEmUtc);

/// <summary>O relato gravado e o que o filtro de privacidade tirou do comentário.</summary>
public sealed record RelatoCriado(long Id, TextoSanitizado? Comentario, IReadOnlyList<AchadoMascarado> Removidos);

/// <summary>Um relato como aparece para todo mundo: sem o autor.</summary>
public sealed record RelatoPublico(long Id, int Nota, IReadOnlyList<Gatilho> Gatilhos, TextoSanitizado? Comentario,
    DateTime VisitadoEm);

public sealed record Pagina<T>(int Numero, int Tamanho, int Total, IReadOnlyList<T> Itens);

public sealed record GatilhoCitado(Gatilho Gatilho, int Vezes);

public sealed record ResumoRelatos(int LugarId, int TotalRelatos, double? NotaMedia, IReadOnlyList<GatilhoCitado> GatilhosMaisCitados);

public interface IRelatoService
{
    /// <summary>Sanitiza o comentário e grava. Usuário ou lugar inexistente: 404; repetido em 30 min: 409.</summary>
    Task<RelatoCriado> CriarAsync(NovoRelato novo, CancellationToken ct = default);

    Task<Pagina<RelatoPublico>> ListarPublicosAsync(int? lugarId, int pagina, int tamanho, CancellationToken ct = default);

    /// <summary>Nota média e gatilhos mais citados; nulo se o lugar não existe.</summary>
    Task<ResumoRelatos?> ResumirAsync(int lugarId, CancellationToken ct = default);
}
