namespace JpHelpDesk.Api.Exceptions;

/// <summary>
/// Exceção de negócio esperada, que deve virar uma resposta HTTP 4xx (e não um erro 500).
/// </summary>
public abstract class AppException(string message, int statusCode, string titulo) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}

/// <summary>Recurso inexistente (HTTP 404).</summary>
public class RecursoNaoEncontradoException(string message)
    : AppException(message, StatusCodes.Status404NotFound, "Recurso não encontrado");

/// <summary>Conflito com o estado atual, ex.: registro duplicado (HTTP 409).</summary>
public class ConflitoException(string message)
    : AppException(message, StatusCodes.Status409Conflict, "Conflito");

/// <summary>Operação que viola uma regra de negócio (HTTP 400).</summary>
public class RegraNegocioException(string message)
    : AppException(message, StatusCodes.Status400BadRequest, "Regra de negócio violada");
