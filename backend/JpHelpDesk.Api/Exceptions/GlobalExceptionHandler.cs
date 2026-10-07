using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Exceptions;

/// <summary>
/// Converte exceções em respostas ProblemDetails (RFC 9457) com o status HTTP adequado.
/// </summary>
public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // Violação de índice único / chave primária no SQL Server.
    private const int SqlErroIndiceUnico = 2601;
    private const int SqlErroChaveDuplicada = 2627;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo, detalhe) = exception switch
        {
            AppException e => (e.StatusCode, e.Titulo, e.Message),
            DbUpdateException { InnerException: SqlException { Number: SqlErroIndiceUnico or SqlErroChaveDuplicada } } =>
                (StatusCodes.Status409Conflict, "Conflito", "O registro viola uma restrição de unicidade."),
            // Requisição que o servidor não conseguiu ler (ex.: corpo truncado ou grande demais): erro do cliente, não 500.
            BadHttpRequestException e =>
                (e.StatusCode, "Requisição inválida", "Não foi possível ler a requisição enviada."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado ao processar a requisição.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado em {Metodo} {Caminho}",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Requisição {Metodo} {Caminho} retornou {Status}: {Mensagem}",
                httpContext.Request.Method, httpContext.Request.Path, status, detalhe);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = titulo,
                Detail = detalhe
            }
        });
    }
}
