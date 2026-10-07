using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace JpHelpDesk.Api.Infrastructure.OpenApi;

/// <summary>
/// Ajusta os tipos de conteúdo documentados ao que a API realmente usa:
/// corpo e respostas de sucesso em "application/json" e erros em "application/problem+json" (RFC 9457).
/// Sem isso o OpenAPI anuncia todos os formatos que o formatter JSON aceita (text/json, application/*+json).
/// </summary>
public class TiposDeConteudoTransformer : IOpenApiOperationTransformer
{
    private const string Json = "application/json";
    private const string ProblemJson = "application/problem+json";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.RequestBody?.Content is { } corpo)
        {
            ManterSomente(corpo, Json);
        }

        foreach (var (codigo, resposta) in operation.Responses ?? [])
        {
            if (resposta.Content is not { Count: > 0 } conteudo || !conteudo.TryGetValue(Json, out var esquema))
            {
                continue;
            }

            var ehErro = int.TryParse(codigo, out var status) && status >= StatusCodes.Status400BadRequest;
            conteudo.Clear();
            conteudo[ehErro ? ProblemJson : Json] = esquema;
        }

        return Task.CompletedTask;
    }

    private static void ManterSomente(IDictionary<string, OpenApiMediaType> conteudo, string tipo)
    {
        if (conteudo.TryGetValue(tipo, out var esquema))
        {
            conteudo.Clear();
            conteudo[tipo] = esquema;
        }
    }
}
