using JpHelpDesk.Api.DTOs.Categorias;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

public class CategoriaService(
    ICategoriaRepository repository,
    IUsuarioAtual usuarioAtual,
    TimeProvider timeProvider) : ICategoriaService
{
    public async Task<ResultadoPaginado<CategoriaResponse>> ListarAsync(CategoriaQuery query, CancellationToken cancellationToken)
    {
        // Somente o ADMIN gerencia categorias inativas; os demais perfis veem só as ativas.
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        if (usuario.Perfil != PerfilUsuario.Admin)
        {
            query.Ativo = true;
        }

        var resultado = await repository.ListarAsync(query, cancellationToken);
        return resultado.Map(CategoriaResponse.FromEntity);
    }

    public async Task<CategoriaResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);
        return CategoriaResponse.FromEntity(categoria);
    }

    public async Task<CategoriaResponse> CriarAsync(CategoriaRequest request, CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        await GarantirNomeUnicoAsync(nome, ignorarId: null, cancellationToken);

        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = NormalizarDescricao(request.Descricao),
            Ativo = true,
            CriadoEm = timeProvider.GetUtcNow()
        };

        repository.Adicionar(categoria);
        await repository.SalvarAlteracoesAsync(cancellationToken);

        return CategoriaResponse.FromEntity(categoria);
    }

    public async Task<CategoriaResponse> AtualizarAsync(int id, CategoriaRequest request, CancellationToken cancellationToken)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);

        var nome = request.Nome.Trim();
        await GarantirNomeUnicoAsync(nome, ignorarId: id, cancellationToken);

        categoria.Nome = nome;
        categoria.Descricao = NormalizarDescricao(request.Descricao);

        await repository.SalvarAlteracoesAsync(cancellationToken);

        return CategoriaResponse.FromEntity(categoria);
    }

    public Task DesativarAsync(int id, CancellationToken cancellationToken) =>
        AlterarSituacaoAsync(id, ativo: false, cancellationToken);

    public Task AtivarAsync(int id, CancellationToken cancellationToken) =>
        AlterarSituacaoAsync(id, ativo: true, cancellationToken);

    private async Task AlterarSituacaoAsync(int id, bool ativo, CancellationToken cancellationToken)
    {
        var categoria = await ObterEntidadeAsync(id, cancellationToken);

        if (categoria.Ativo == ativo)
        {
            return; // Operação idempotente: já está na situação desejada.
        }

        categoria.Ativo = ativo;
        await repository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Categoria> ObterEntidadeAsync(int id, CancellationToken cancellationToken) =>
        await repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException($"Categoria {id} não encontrada.");

    private async Task GarantirNomeUnicoAsync(string nome, int? ignorarId, CancellationToken cancellationToken)
    {
        if (await repository.NomeExisteAsync(nome, ignorarId, cancellationToken))
        {
            throw new ConflitoException($"Já existe uma categoria com o nome '{nome}'.");
        }
    }

    private static string? NormalizarDescricao(string? descricao) =>
        string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
}
