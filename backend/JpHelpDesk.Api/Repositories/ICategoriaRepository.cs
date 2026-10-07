using JpHelpDesk.Api.DTOs.Categorias;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;

namespace JpHelpDesk.Api.Repositories;

public interface ICategoriaRepository
{
    Task<ResultadoPaginado<Categoria>> ListarAsync(CategoriaQuery query, CancellationToken cancellationToken);
    Task<Categoria?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> NomeExisteAsync(string nome, int? ignorarId, CancellationToken cancellationToken);
    void Adicionar(Categoria categoria);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
