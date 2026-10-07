using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Models;

public class Chamado
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public StatusChamado Status { get; set; } = StatusChamado.Aberto;
    public PrioridadeChamado Prioridade { get; set; } = PrioridadeChamado.Media;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int SolicitanteId { get; set; }
    public Usuario Solicitante { get; set; } = null!;

    public int? TecnicoId { get; set; }
    public Usuario? Tecnico { get; set; }

    public DateTimeOffset DataAbertura { get; set; }
    public DateTimeOffset? DataAtualizacao { get; set; }
    public DateTimeOffset? DataFechamento { get; set; }

    public ICollection<HistoricoChamado> Historicos { get; set; } = [];
}
