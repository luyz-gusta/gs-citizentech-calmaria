using Calmaria.Domain.Comum;
using Calmaria.Domain.Comunidade;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Privacidade.Taint;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Application.Comunidade;

/// <summary>Usuário anônimo e perfil sensorial.</summary>
internal sealed class UsuarioService(CalmariaDbContext db, ISanitizador sanitizador, TimeProvider relogio)
    : IUsuarioService
{
    public async Task<Guid> CriarAsync(TextoNaoConfiavel? apelido, CancellationToken ct = default)
    {
        if (apelido is not null && apelido.Tamanho > Usuario.TamanhoMaximoDoApelido)
            throw new DominioException($"O apelido passa de {Usuario.TamanhoMaximoDoApelido} caracteres.");

        // Até o apelido passa pelo Sanitizador: alguém pode escrever o próprio telefone ali.
        var limpo = apelido is null ? null : sanitizador.Sanitizar(apelido).Texto;
        var usuario = Usuario.Criar(limpo, relogio.GetUtcNow().UtcDateTime);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
        return usuario.Id;
    }

    public async Task<DadosDoPerfil?> ObterPerfilAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.PerfisSensoriais.AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => new DadosDoPerfil(p.SensibilidadeSom, p.SensibilidadeLuz, p.SensibilidadeMultidao,
                p.ContatoApoioNome, p.ContatoApoioTelefone))
            .FirstOrDefaultAsync(ct);

    public async Task SalvarPerfilAsync(Guid usuarioId, DadosDoPerfil dados, CancellationToken ct = default)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == usuarioId, ct))
            throw UsuarioNaoEncontrado(usuarioId);

        var perfil = await db.PerfisSensoriais.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, ct);
        if (perfil is null)
        {
            perfil = new PerfilSensorial(usuarioId);
            db.PerfisSensoriais.Add(perfil);
        }

        perfil.Atualizar(
            new Sensibilidade(dados.SensibilidadeSom, "som"),
            new Sensibilidade(dados.SensibilidadeLuz, "luz"),
            new Sensibilidade(dados.SensibilidadeMultidao, "multidão"),
            dados.ContatoApoioNome, dados.ContatoApoioTelefone, relogio.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExcluirAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
        if (usuario is null)
            return false;

        // O banco apaga em cascata o perfil e os relatos.
        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync(ct);
        return true;
    }

    internal static RecursoNaoEncontradoException UsuarioNaoEncontrado(Guid id) =>
        new("Usuário não encontrado", $"Não existe usuário com o id {id}.");
}
