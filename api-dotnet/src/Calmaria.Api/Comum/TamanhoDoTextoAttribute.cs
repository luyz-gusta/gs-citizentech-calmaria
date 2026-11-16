using System.ComponentModel.DataAnnotations;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Api.Comum;

/// <summary>Limite de tamanho para um texto não confiável, sem ler o conteúdo dele.</summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class TamanhoDoTextoAttribute(int maximo) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        value is TextoNaoConfiavel texto && texto.Tamanho > maximo
            ? new ValidationResult($"O texto passa de {maximo} caracteres.", [validationContext.MemberName ?? "texto"])
            : ValidationResult.Success;
}
