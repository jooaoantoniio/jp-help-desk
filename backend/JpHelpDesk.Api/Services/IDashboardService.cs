using JpHelpDesk.Api.DTOs.Dashboard;

namespace JpHelpDesk.Api.Services;

public interface IDashboardService
{
    Task<DashboardResponse> ObterAsync(CancellationToken cancellationToken);
}
