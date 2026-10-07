using JpHelpDesk.Api.Models;

namespace JpHelpDesk.Api.DTOs.Categorias;

/// <summary>
/// Categoria retornada pela API.
/// </summary>
public record CategoriaResponse(
    int Id,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTimeOffset CriadoEm)
{
    public static CategoriaResponse FromEntity(Categoria categoria) => new(
        categoria.Id,
        categoria.Nome,
        categoria.Descricao,
        categoria.Ativo,
        categoria.CriadoEm);
}
