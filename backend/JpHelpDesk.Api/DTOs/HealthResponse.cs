namespace JpHelpDesk.Api.DTOs;

/// <summary>
/// Informações de saúde da API.
/// </summary>
/// <param name="Status">Situação da API (ex.: "Healthy").</param>
/// <param name="Version">Versão da aplicação.</param>
/// <param name="Environment">Ambiente em execução (Development, Production...).</param>
/// <param name="Timestamp">Data e hora (UTC) da verificação.</param>
public record HealthResponse(
    string Status,
    string Version,
    string Environment,
    DateTimeOffset Timestamp);
