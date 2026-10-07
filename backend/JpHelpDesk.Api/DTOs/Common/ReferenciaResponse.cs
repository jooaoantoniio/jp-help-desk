namespace JpHelpDesk.Api.DTOs.Common;

/// <summary>
/// Referência resumida a outro recurso (ID + nome), usada dentro de outras respostas.
/// </summary>
public record ReferenciaResponse(int Id, string Nome);
