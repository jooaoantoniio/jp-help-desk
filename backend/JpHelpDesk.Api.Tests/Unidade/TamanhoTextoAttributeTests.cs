using JpHelpDesk.Api.DTOs.Common;

namespace JpHelpDesk.Api.Tests.Unidade;

public class TamanhoTextoAttributeTests
{
    private readonly TamanhoTextoAttribute _atributo = new(10) { MinimumLength = 3 };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")] // vazio é assunto do [Required]
    [InlineData("abc")]
    [InlineData("abcdefghij")]
    public void Valido(string? valor) => Assert.True(_atributo.IsValid(valor));

    [Theory]
    [InlineData("ab")]
    [InlineData("abcdefghijk")]
    public void Invalido(string valor) => Assert.False(_atributo.IsValid(valor));
}
