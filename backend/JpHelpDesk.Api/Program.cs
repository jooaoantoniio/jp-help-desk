using System.Text.Json;
using System.Text.Json.Serialization;
using JpHelpDesk.Api.Data;
using JpHelpDesk.Api.Data.Seed;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Infrastructure.Authentication;
using JpHelpDesk.Api.Infrastructure.ModelBinding;
using JpHelpDesk.Api.Infrastructure.OpenApi;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services;
using JpHelpDesk.Api.Services.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serviços (injeção de dependência) ----------

// Enums trafegam no JSON como texto UPPER_SNAKE_CASE (ex.: "EM_ATENDIMENTO"), iguais ao banco.
// Números não são aceitos, para evitar valores inválidos como "perfil": 99.
var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper, allowIntegerValues: false);

builder.Services.AddControllers(options =>
    {
        // Enums na query string/rota no mesmo formato do JSON (ex.: ?status=EM_ATENDIMENTO).
        options.ModelBinderProviders.Insert(0, new UpperSnakeCaseEnumModelBinderProvider());
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(enumConverter));

// O gerador do OpenAPI lê estas opções para descrever os enums no Swagger.
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(enumConverter));

// Banco de dados: a connection string vem da configuração do ambiente
// (appsettings.Development.json em dev; variável de ambiente em produção).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Relógio do sistema injetável: facilita testar código que depende de data/hora.
builder.Services.AddSingleton(TimeProvider.System);

// Segurança: hash de senha (sem estado, pode ser Singleton).
builder.Services.AddSingleton<ISenhaHasher, BCryptSenhaHasher>();

// Autenticação JWT, autorização (protegido por padrão), usuário atual e limite de tentativas de login.
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

// Repositórios e serviços (Scoped = uma instância por requisição HTTP).
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<IChamadoService, ChamadoService>();

// Seed de dados de desenvolvimento.
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(SeedOptions.Secao));
builder.Services.AddScoped<DatabaseSeeder>();

// Erros retornados no formato padrão ProblemDetails (RFC 9457).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

    // Botão "Authorize" do Swagger para enviar o token JWT.
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// ---------- Inicialização ----------

if (app.Environment.IsDevelopment())
{
    // Fora de uma requisição não existe escopo: criamos um para usar serviços Scoped.
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
}

// ---------- Pipeline HTTP (a ordem dos middlewares importa) ----------

// Primeiro da fila: captura exceções de todos os middlewares seguintes.
app.UseExceptionHandler();
// Respostas de erro sem corpo (ex.: 404 de rota inexistente) também viram ProblemDetails.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // A documentação é pública em desenvolvimento (a política padrão exige login).
    app.MapOpenApi().AllowAnonymous();

    // Interface do Swagger lendo o documento OpenAPI gerado acima.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "JP Help Desk API v1");
        options.DocumentTitle = "JP Help Desk API";
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

// Autenticação (quem é você?) sempre antes da autorização (o que você pode fazer?).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
