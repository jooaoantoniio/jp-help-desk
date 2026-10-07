using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Models;

public class HistoricoChamado
{
    public int Id { get; set; }

    public int ChamadoId { get; set; }
    public Chamado Chamado { get; set; } = null!;

    /// <summary>Usuário que realizou a ação.</summary>
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public TipoHistorico Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTimeOffset DataRegistro { get; set; }
}
