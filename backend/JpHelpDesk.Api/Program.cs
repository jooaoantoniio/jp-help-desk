using JpHelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serviços (injeção de dependência) ----------

builder.Services.AddControllers();

// Banco de dados: a connection string vem da configuração do ambiente
// (appsettings.Development.json em dev; variável de ambiente em produção).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Relógio do sistema injetável: facilita testar código que depende de data/hora.
builder.Services.AddSingleton(TimeProvider.System);

// Documento OpenAPI nativo do .NET (servido em /openapi/v1.json).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "JP Help Desk API";
        document.Info.Version = "v1";
        document.Info.Description = "API REST para gerenciamento de chamados de suporte técnico.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// ---------- Pipeline HTTP (a ordem dos middlewares importa) ----------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Interface do Swagger lendo o documento OpenAPI gerado acima.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "JP Help Desk API v1");
        options.DocumentTitle = "JP Help Desk API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
