using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Evento da linha do tempo de um chamado.
/// </summary>
public record HistoricoResponse(
    int Id,
    TipoHistorico Tipo,
    string Descricao,
    ReferenciaResponse Usuario,
    DateTimeOffset DataRegistro)
{
    /// <summary>Requer a navegação Usuario carregada.</summary>
    public static HistoricoResponse FromEntity(HistoricoChamado historico) => new(
        historico.Id,
        historico.Tipo,
        historico.Descricao,
        new ReferenciaResponse(historico.Usuario.Id, historico.Usuario.Nome),
        historico.DataRegistro);
}
