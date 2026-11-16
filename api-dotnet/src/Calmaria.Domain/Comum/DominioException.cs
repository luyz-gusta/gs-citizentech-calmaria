namespace Calmaria.Domain.Comum;

/// <summary>Regra de negócio violada. A API traduz para um ProblemDetails 400 com a mensagem.</summary>
public class DominioException(string mensagem) : Exception(mensagem);

/// <summary>Valor físico impossível para o sensor (som, luz ou movimento fora da faixa aceita). Vira 422.</summary>
public sealed class ValorForaDaFaixaException(string campo, double valor, double minimo, double maximo)
    : DominioException($"{campo} deve estar entre {minimo} e {maximo}; veio {valor}.")
{
    public string Campo { get; } = campo;
}

/// <summary>A operação bate com algo que já existe (relato repetido em 30 minutos). Vira 409.</summary>
public sealed class ConflitoException(string titulo, string mensagem) : DominioException(mensagem)
{
    public string Titulo { get; } = titulo;
}

/// <summary>O recurso pedido não existe (lugar, dispositivo, usuário). Vira 404.</summary>
public sealed class RecursoNaoEncontradoException(string titulo, string mensagem) : DominioException(mensagem)
{
    public string Titulo { get; } = titulo;
}
