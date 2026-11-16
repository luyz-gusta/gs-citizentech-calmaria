using Calmaria.Domain.Comum;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Domain.Comunidade;

/// <summary>O que incomodou na visita (contrato: BARULHO, LUZ_FORTE, MULTIDAO, CHEIRO, FILA, OUTRO).</summary>
public enum Gatilho
{
    Barulho,
    LuzForte,
    Multidao,
    Cheiro,
    Fila,
    Outro
}

/// <summary>Quanto um estímulo incomoda a pessoa, de 1 (quase nada) a 5 (não consegue ficar).</summary>
public readonly record struct Sensibilidade
{
    public int Valor { get; }

    public Sensibilidade(int valor, string nome)
    {
        if (valor is < 1 or > 5)
            throw new DominioException($"A sensibilidade a {nome} deve estar entre 1 e 5.");
        Valor = valor;
    }
}

/// <summary>A nota da visita, de 1 (insuportável) a 5 (muito calmo).</summary>
public readonly record struct NotaSensorial
{
    public int Valor { get; }

    public NotaSensorial(int valor)
    {
        if (valor is < 1 or > 5)
            throw new DominioException("A nota deve estar entre 1 e 5.");
        Valor = valor;
    }
}

/// <summary>
/// Usuário anônimo: só um identificador e, se a pessoa quiser, um apelido. Sem nome, e-mail, CPF nem senha: o
/// anonimato é decisão de projeto.
/// </summary>
public sealed class Usuario
{
    public const int TamanhoMaximoDoApelido = 30;

    public Guid Id { get; private set; }
    public TextoSanitizado? Apelido { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Usuario() { }

    public static Usuario Criar(TextoSanitizado? apelido, DateTime agoraUtc) => new()
    {
        Id = Guid.NewGuid(),
        Apelido = apelido is null || string.IsNullOrWhiteSpace(apelido.Valor) ? null : apelido,
        CriadoEm = DateTime.SpecifyKind(agoraUtc, DateTimeKind.Utc)
    };
}

/// <summary>
/// O perfil sensorial, um por usuário. O contato de apoio é dado pessoal: fica só aqui, nunca vai para log nem
/// para resposta pública.
/// </summary>
public sealed class PerfilSensorial
{
    public Guid UsuarioId { get; private set; }
    public int SensibilidadeSom { get; private set; }
    public int SensibilidadeLuz { get; private set; }
    public int SensibilidadeMultidao { get; private set; }
    public string? ContatoApoioNome { get; private set; }
    public string? ContatoApoioTelefone { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    private PerfilSensorial() { }

    public PerfilSensorial(Guid usuarioId) => UsuarioId = usuarioId;

    public void Atualizar(Sensibilidade som, Sensibilidade luz, Sensibilidade multidao, string? contatoNome,
        string? contatoTelefone, DateTime agoraUtc)
    {
        SensibilidadeSom = som.Valor;
        SensibilidadeLuz = luz.Valor;
        SensibilidadeMultidao = multidao.Valor;
        ContatoApoioNome = string.IsNullOrWhiteSpace(contatoNome) ? null : contatoNome.Trim();
        ContatoApoioTelefone = string.IsNullOrWhiteSpace(contatoTelefone) ? null : contatoTelefone.Trim();
        AtualizadoEm = DateTime.SpecifyKind(agoraUtc, DateTimeKind.Utc);
    }
}

/// <summary>Um gatilho citado num relato (tabela RelatoGatilhos).</summary>
public sealed class RelatoGatilho
{
    public Gatilho Gatilho { get; private set; }

    private RelatoGatilho() { }

    public RelatoGatilho(Gatilho gatilho) => Gatilho = gatilho;
}

/// <summary>
/// Como foi a visita a um lugar. O comentário só entra como <see cref="TextoSanitizado"/>: gravar o texto cru do
/// cidadão não compila.
/// </summary>
public sealed class Relato
{
    public const int TamanhoMaximoDoComentario = 500;

    /// <summary>Folga para relógio de celular adiantado.</summary>
    private static readonly TimeSpan FolgaDoRelogio = TimeSpan.FromMinutes(5);

    private readonly List<RelatoGatilho> gatilhos = [];

    public long Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public int LugarId { get; private set; }
    public int Nota { get; private set; }
    public IReadOnlyCollection<RelatoGatilho> Gatilhos => gatilhos;
    public TextoSanitizado? Comentario { get; private set; }
    public DateTime VisitadoEm { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Relato() { }

    public static Relato Criar(Guid usuarioId, int lugarId, NotaSensorial nota, IEnumerable<Gatilho> gatilhos,
        TextoSanitizado? comentario, DateTime visitadoEmUtc, DateTime agoraUtc)
    {
        if (visitadoEmUtc > agoraUtc + FolgaDoRelogio)
            throw new DominioException("A data da visita não pode estar no futuro.");

        var relato = new Relato
        {
            UsuarioId = usuarioId,
            LugarId = lugarId,
            Nota = nota.Valor,
            Comentario = comentario is null || string.IsNullOrWhiteSpace(comentario.Valor) ? null : comentario,
            VisitadoEm = DateTime.SpecifyKind(visitadoEmUtc, DateTimeKind.Utc),
            CriadoEm = DateTime.SpecifyKind(agoraUtc, DateTimeKind.Utc)
        };
        relato.gatilhos.AddRange(gatilhos.Distinct().Select(g => new RelatoGatilho(g)));
        return relato;
    }
}
