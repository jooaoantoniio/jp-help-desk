using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.DTOs.Common;

/// <summary>
/// Igual ao [StringLength], mas ignora texto vazio ou só com espaços — esse caso é do [Required].
/// Assim um campo vazio recebe só "é obrigatório", e não também "deve ter entre X e Y caracteres".
/// Por herdar de StringLengthAttribute, o OpenAPI continua documentando minLength/maxLength.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class TamanhoTextoAttribute(int maximumLength) : StringLengthAttribute(maximumLength)
{
    public override bool IsValid(object? value) =>
        value is string texto && string.IsNullOrWhiteSpace(texto) || base.IsValid(value);
}
