using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Dashboard;

/// <summary>
/// Indicadores do dashboard. Para o perfil USUARIO, considera apenas os chamados que ele abriu.
/// </summary>
public record DashboardResponse(
    ResumoDashboard Resumo,
    IReadOnlyList<ContagemStatus> PorStatus,
    IReadOnlyList<ContagemPrioridade> PorPrioridade,
    IReadOnlyList<ContagemCategoria> PorCategoria,
    IReadOnlyList<ChamadoResponse> Recentes);

/// <summary>
/// Totais principais. "CriticosEmAberto" = prioridade CRITICA ainda não resolvida;
/// "Resolvidos" = RESOLVIDO + FECHADO.
/// </summary>
public record ResumoDashboard(
    int Total,
    int Abertos,
    int EmAtendimento,
    int AguardandoUsuario,
    int CriticosEmAberto,
    int Resolvidos);

public record ContagemStatus(StatusChamado Status, int Quantidade);

public record ContagemPrioridade(PrioridadeChamado Prioridade, int Quantidade);

public record ContagemCategoria(ReferenciaResponse Categoria, int Quantidade);
