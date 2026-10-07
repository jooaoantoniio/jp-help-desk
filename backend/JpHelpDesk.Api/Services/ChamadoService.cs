using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

public class ChamadoService(
    IChamadoRepository chamadoRepository,
    ICategoriaRepository categoriaRepository,
    IUsuarioRepository usuarioRepository,
    IUsuarioAtual usuarioAtual,
    TimeProvider timeProvider) : IChamadoService
{
    public async Task<ResultadoPaginado<ChamadoResponse>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken)
    {
        var resultado = await chamadoRepository.ListarAsync(query, cancellationToken);
        return resultado.Map(ChamadoResponse.FromEntity);
    }

    public async Task<ChamadoResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var chamado = await ObterEntidadeAsync(id, cancellationToken);
        return ChamadoResponse.FromEntity(chamado);
    }

    public async Task<ChamadoResponse> AbrirAsync(ChamadoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var categoria = await ObterCategoriaAtivaAsync(request.CategoriaId!.Value, cancellationToken);
        var solicitante = await usuarioRepository.ObterPorIdAsync(usuario.Id, cancellationToken)
            ?? throw new NaoAutenticadoException("Usuário não encontrado.");

        var chamado = new Chamado
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            Prioridade = request.Prioridade!.Value,
            Status = StatusChamado.Aberto,
            Categoria = categoria,
            Solicitante = solicitante,
            DataAbertura = timeProvider.GetUtcNow()
        };

        chamadoRepository.Adicionar(chamado);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado);
    }

    public async Task<ChamadoResponse> AtualizarAsync(int id, ChamadoRequest request, CancellationToken cancellationToken)
    {
        var chamado = await ObterEntidadeAsync(id, cancellationToken);
        GarantirNaoFinalizado(chamado);

        // Só exige categoria ativa se ela estiver sendo trocada.
        if (chamado.CategoriaId != request.CategoriaId)
        {
            chamado.Categoria = await ObterCategoriaAtivaAsync(request.CategoriaId!.Value, cancellationToken);
        }

        chamado.Titulo = request.Titulo.Trim();
        chamado.Descricao = request.Descricao.Trim();
        chamado.Prioridade = request.Prioridade!.Value;
        chamado.DataAtualizacao = timeProvider.GetUtcNow();

        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado);
    }

    public async Task<ChamadoResponse> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken cancellationToken)
    {
        var chamado = await ObterEntidadeAsync(id, cancellationToken);

        AplicarStatus(chamado, request.Status!.Value);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado);
    }

    public async Task<ChamadoResponse> AtribuirTecnicoAsync(int id, AtribuirTecnicoRequest request, CancellationToken cancellationToken)
    {
        var chamado = await ObterEntidadeAsync(id, cancellationToken);

        if (!FluxoStatusChamado.PermiteAtribuicao(chamado.Status))
        {
            throw new RegraNegocioException(
                $"Chamados com status {chamado.Status.ToApiString()} não podem ser atribuídos.");
        }

        var tecnico = await usuarioRepository.ObterPorIdAsync(request.TecnicoId!.Value, cancellationToken);

        if (tecnico is null || !tecnico.Ativo || tecnico.Perfil == PerfilUsuario.Usuario)
        {
            throw new RegraNegocioException(
                $"Usuário {request.TecnicoId} não encontrado, inativo ou sem perfil TECNICO/ADMIN.");
        }

        chamado.Tecnico = tecnico;
        chamado.DataAtualizacao = timeProvider.GetUtcNow();

        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado);
    }

    public async Task CancelarAsync(int id, CancellationToken cancellationToken)
    {
        var chamado = await ObterEntidadeAsync(id, cancellationToken);

        if (chamado.Status == StatusChamado.Cancelado)
        {
            return; // Idempotente.
        }

        AplicarStatus(chamado, StatusChamado.Cancelado);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>
    /// Valida a transição na máquina de estados e atualiza status e datas.
    /// </summary>
    private void AplicarStatus(Chamado chamado, StatusChamado novoStatus)
    {
        if (!FluxoStatusChamado.PodeAlterar(chamado.Status, novoStatus))
        {
            var permitidos = FluxoStatusChamado.ProximosStatus(chamado.Status);
            var opcoes = permitidos.Count == 0
                ? "o status atual é final"
                : $"permitidos: {string.Join(", ", permitidos.Select(s => s.ToApiString()))}";

            throw new RegraNegocioException(
                $"Não é possível alterar o status de {chamado.Status.ToApiString()} para {novoStatus.ToApiString()} ({opcoes}).");
        }

        if (novoStatus == StatusChamado.EmAtendimento && chamado.TecnicoId is null)
        {
            throw new RegraNegocioException("Atribua um técnico antes de iniciar o atendimento.");
        }

        var agora = timeProvider.GetUtcNow();
        chamado.Status = novoStatus;
        chamado.DataAtualizacao = agora;
        chamado.DataFechamento = FluxoStatusChamado.EhFinal(novoStatus) ? agora : null;
    }

    private async Task<Chamado> ObterEntidadeAsync(int id, CancellationToken cancellationToken) =>
        await chamadoRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException($"Chamado {id} não encontrado.");

    private async Task<Categoria> ObterCategoriaAtivaAsync(int categoriaId, CancellationToken cancellationToken)
    {
        var categoria = await categoriaRepository.ObterPorIdAsync(categoriaId, cancellationToken);

        if (categoria is null || !categoria.Ativo)
        {
            throw new RegraNegocioException($"Categoria {categoriaId} não encontrada ou inativa.");
        }

        return categoria;
    }

    private static void GarantirNaoFinalizado(Chamado chamado)
    {
        if (FluxoStatusChamado.EhFinal(chamado.Status))
        {
            throw new RegraNegocioException(
                $"Chamados com status {chamado.Status.ToApiString()} não podem ser editados.");
        }
    }
}
