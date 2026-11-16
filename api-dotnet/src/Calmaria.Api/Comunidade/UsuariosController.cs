using System.ComponentModel.DataAnnotations;
using Calmaria.Api.Comum;
using Calmaria.Domain.Comunidade;
using Calmaria.Privacidade.Taint;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Comunidade;

/// <summary>Usuário anônimo (sem nome, e-mail, CPF nem senha) e perfil sensorial.</summary>
[ApiController]
[Route("api/usuarios")]
[Tags("Comunidade")]
public sealed class UsuariosController(IUsuarioService usuarios) : ControllerBase
{
    /// <summary>Cria um usuário anônimo. O app guarda o id no celular.</summary>
    /// <param name="corpo">Vazio, ou com um apelido de até 30 caracteres.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="201">O id do usuário.</response>
    /// <response code="400">Apelido longo demais.</response>
    [HttpPost]
    [ProducesResponseType<UsuarioCriadoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Criar([FromBody] NovoUsuarioDto? corpo, CancellationToken ct)
    {
        var id = await usuarios.CriarAsync(corpo?.Apelido, ct);
        return Created($"/api/usuarios/{id}/perfil-sensorial", new UsuarioCriadoDto(id));
    }

    /// <summary>O perfil sensorial do usuário.</summary>
    /// <param name="id">O id do usuário.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O perfil.</response>
    /// <response code="404">Usuário sem perfil ou que não existe.</response>
    [HttpGet("{id:guid}/perfil-sensorial")]
    [ProducesResponseType<PerfilSensorialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> ObterPerfil(Guid id, CancellationToken ct)
    {
        var perfil = await usuarios.ObterPerfilAsync(id, ct);
        return perfil is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Perfil não encontrado",
                detail: "Este usuário não existe ou ainda não preencheu o perfil sensorial.")
            : Ok(PerfilSensorialDto.De(perfil));
    }

    /// <summary>Grava o perfil sensorial (cria ou substitui).</summary>
    /// <param name="id">O id do usuário.</param>
    /// <param name="corpo">Sensibilidades de 1 a 5 e o contato de apoio (opcional).</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="204">Gravado.</response>
    /// <response code="400">Sensibilidade fora de 1 a 5 ou telefone inválido.</response>
    /// <response code="404">O usuário não existe.</response>
    [HttpPut("{id:guid}/perfil-sensorial")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> SalvarPerfil(Guid id, [FromBody] PerfilSensorialDto corpo, CancellationToken ct)
    {
        await usuarios.SalvarPerfilAsync(id, corpo.ParaDados(), ct);
        return NoContent();
    }

    /// <summary>Apaga o usuário, o perfil e todos os relatos dele (direito de exclusão da LGPD, art. 18).</summary>
    /// <param name="id">O id do usuário.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="204">Apagado.</response>
    /// <response code="404">O usuário não existe.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct) =>
        await usuarios.ExcluirAsync(id, ct)
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: "Usuário não encontrado",
                detail: $"Não existe usuário com o id {id}.");
}

/// <summary>Usuário novo. O apelido é opcional e passa pelo filtro de privacidade.</summary>
public sealed record NovoUsuarioDto([TamanhoDoTexto(30)] TextoNaoConfiavel? Apelido);

public sealed record UsuarioCriadoDto(Guid Id);

/// <summary>O perfil sensorial: quanto cada estímulo incomoda, de 1 (quase nada) a 5 (não consegue ficar).</summary>
/// <param name="SensibilidadeSom">De 1 a 5.</param>
/// <param name="SensibilidadeLuz">De 1 a 5.</param>
/// <param name="SensibilidadeMultidao">De 1 a 5.</param>
/// <param name="ContatoApoioNome">Quem chamar na crise (opcional, até 60 caracteres).</param>
/// <param name="ContatoApoioTelefone">Telefone do contato, só dígitos, com DDD (opcional).</param>
public sealed record PerfilSensorialDto(
    [Range(1, 5)] int SensibilidadeSom,
    [Range(1, 5)] int SensibilidadeLuz,
    [Range(1, 5)] int SensibilidadeMultidao,
    [MaxLength(60)] string? ContatoApoioNome,
    [RegularExpression(@"^\+?\d{10,13}$", ErrorMessage = "Use só dígitos, com DDD (10 a 13).")] string? ContatoApoioTelefone)
{
    public DadosDoPerfil ParaDados() => new(SensibilidadeSom, SensibilidadeLuz, SensibilidadeMultidao,
        ContatoApoioNome, ContatoApoioTelefone);

    public static PerfilSensorialDto De(DadosDoPerfil d) => new(d.SensibilidadeSom, d.SensibilidadeLuz,
        d.SensibilidadeMultidao, d.ContatoApoioNome, d.ContatoApoioTelefone);
}
