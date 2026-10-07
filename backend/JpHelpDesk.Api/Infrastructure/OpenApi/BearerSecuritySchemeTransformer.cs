using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace JpHelpDesk.Api.Infrastructure.OpenApi;

/// <summary>
/// Declara a autenticação JWT Bearer no documento OpenAPI — habilita o botão "Authorize" do Swagger.
/// </summary>
public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private const string Esquema = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[Esquema] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Faça login em POST /api/auth/login e cole aqui o token retornado."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(Esquema, document)] = []
        });

        return Task.CompletedTask;
    }
}
