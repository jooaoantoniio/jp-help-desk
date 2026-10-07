using JpHelpDesk.Api.Data;
using JpHelpDesk.Api.Data.Extensions;
using JpHelpDesk.Api.DTOs.Categorias;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Repositories;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public async Task<ResultadoPaginado<Categoria>> ListarAsync(CategoriaQuery query, CancellationToken cancellationToken)
    {
        // AsNoTracking: leitura pura, o EF não precisa monitorar alterações (mais rápido).
        var consulta = context.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            consulta = consulta.Where(c => c.Nome.Contains(query.Busca.Trim()));
        }

        if (query.Ativo.HasValue)
        {
            consulta = consulta.Where(c => c.Ativo == query.Ativo.Value);
        }

        return await consulta
            .OrderBy(c => c.Nome)
            .ToResultadoPaginadoAsync(query, cancellationToken);
    }

    public Task<Categoria?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NomeExisteAsync(string nome, int? ignorarId, CancellationToken cancellationToken) =>
        context.Categorias.AnyAsync(c => c.Nome == nome && c.Id != ignorarId, cancellationToken);

    public void Adicionar(Categoria categoria) => context.Categorias.Add(categoria);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
