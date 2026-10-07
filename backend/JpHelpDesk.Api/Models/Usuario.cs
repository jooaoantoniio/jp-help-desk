using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset? AtualizadoEm { get; set; }

    public ICollection<Chamado> ChamadosSolicitados { get; set; } = [];
    public ICollection<Chamado> ChamadosAtribuidos { get; set; } = [];
    public ICollection<HistoricoChamado> Historicos { get; set; } = [];
}
