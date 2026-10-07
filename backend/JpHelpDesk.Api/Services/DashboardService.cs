using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Dashboard;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

/// <summary>
/// Monta os indicadores do dashboard. A equipe (ADMIN/TECNICO) vê todos os chamados;
/// o USUARIO vê apenas os números dos chamados que abriu.
/// </summary>
public class DashboardService(IChamadoRepository chamadoRepository, IUsuarioAtual usuarioAtual) : IDashboardService
{
    private const int QuantidadeRecentes = 5;

    public async Task<DashboardResponse> ObterAsync(CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        int? solicitanteId = PermissoesChamado.EhEquipe(usuario) ? null : usuario.Id;

        // Consultas sequenciais: o DbContext não permite operações simultâneas na mesma instância.
        var porStatus = await chamadoRepository.ContarPorStatusAsync(solicitanteId, cancellationToken);
        var porPrioridade = await chamadoRepository.ContarPorPrioridadeAsync(solicitanteId, cancellationToken);
        var porCategoria = await chamadoRepository.ContarPorCategoriaAsync(solicitanteId, cancellationToken);
        var criticosEmAberto = await chamadoRepository.ContarCriticosEmAbertoAsync(solicitanteId, cancellationToken);
        var recentes = await chamadoRepository.ListarAsync(
            new ChamadoQuery { SolicitanteId = solicitanteId, TamanhoPagina = QuantidadeRecentes },
            cancellationToken);

        int Status(StatusChamado status) => porStatus.GetValueOrDefault(status);

        var resumo = new ResumoDashboard(
            Total: porStatus.Values.Sum(),
            Abertos: Status(StatusChamado.Aberto),
            EmAtendimento: Status(StatusChamado.EmAtendimento),
            AguardandoUsuario: Status(StatusChamado.AguardandoUsuario),
            CriticosEmAberto: criticosEmAberto,
            Resolvidos: Status(StatusChamado.Resolvido) + Status(StatusChamado.Fechado));

        // Todos os valores do enum aparecem (inclusive com zero), na ordem do ciclo de vida.
        return new DashboardResponse(
            resumo,
            Enum.GetValues<StatusChamado>().Select(s => new ContagemStatus(s, Status(s))).ToList(),
            Enum.GetValues<PrioridadeChamado>().Reverse()
                .Select(p => new ContagemPrioridade(p, porPrioridade.GetValueOrDefault(p))).ToList(),
            porCategoria.Select(c => new ContagemCategoria(new ReferenciaResponse(c.CategoriaId, c.Categoria), c.Quantidade)).ToList(),
            recentes.Itens.Select(c => ChamadoResponse.FromEntity(c, usuario)).ToList());
    }
}
