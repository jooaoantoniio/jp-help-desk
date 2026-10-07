using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace JpHelpDesk.Api.Infrastructure.ErrorHandling;

/// <summary>
/// Padroniza todas as respostas de erro da API no formato ProblemDetails (RFC 9457), em português.
/// </summary>
public static class RespostasErroExtensions
{
    /// <summary>
    /// Título e explicação para erros que o próprio ASP.NET Core gera sem detalhe
    /// (rota inexistente, token ausente, perfil sem permissão, limite de requisições...).
    /// </summary>
    private static readonly Dictionary<int, (string Titulo, string Detalhe)> MensagensPorStatus = new()
    {
        [StatusCodes.Status400BadRequest] = ("Requisição inválida", "A requisição não pôde ser processada."),
        [StatusCodes.Status401Unauthorized] = ("Não autenticado", "Faça login e envie o token no cabeçalho \"Authorization: Bearer {token}\"."),
        [StatusCodes.Status403Forbidden] = ("Acesso negado", "Seu perfil não tem permissão para acessar este recurso."),
        [StatusCodes.Status404NotFound] = ("Recurso não encontrado", "O endereço solicitado não existe."),
        [StatusCodes.Status405MethodNotAllowed] = ("Método não permitido", "Este endereço não aceita o método HTTP utilizado."),
        [StatusCodes.Status415UnsupportedMediaType] = ("Tipo de conteúdo não suportado", "Envie o corpo em JSON com o cabeçalho \"Content-Type: application/json\"."),
        [StatusCodes.Status429TooManyRequests] = ("Muitas requisições", "Limite de tentativas atingido. Aguarde um minuto e tente novamente."),
        [StatusCodes.Status500InternalServerError] = ("Erro interno", "Ocorreu um erro inesperado ao processar a requisição.")
    };

    /// <summary>ProblemDetails para erros sem corpo, com título e detalhe em português.</summary>
    public static IServiceCollection AddRespostasDeErro(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = contexto =>
        {
            var problema = contexto.ProblemDetails;

            // Erros que já trazem explicação (ex.: exceções da aplicação) ficam como estão.
            if (problema.Detail is null && problema.Status is int status && MensagensPorStatus.TryGetValue(status, out var padrao))
            {
                problema.Title = padrao.Titulo;
                problema.Detail = padrao.Detalhe;
            }
        });

        return services;
    }

    /// <summary>Respostas de validação (400) padronizadas e mensagens de model binding em português.</summary>
    public static IMvcBuilder ConfigurarRespostasDeValidacao(this IMvcBuilder mvc)
    {
        mvc.ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = RespostaValidacao.Criar);

        // Mensagens internas do System.Text.Json citam tipos .NET; nunca devolvê-las ao cliente.
        mvc.AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false);

        mvc.AddMvcOptions(options => TraduzirMensagensDeBinding(options.ModelBindingMessageProvider));

        return mvc;
    }

    /// <summary>Mensagens de conversão da query string/rota (ex.: ?pagina=abc). O campo vai na chave do erro.</summary>
    private static void TraduzirMensagensDeBinding(DefaultModelBindingMessageProvider mensagens)
    {
        mensagens.SetAttemptedValueIsInvalidAccessor((valor, _) => $"O valor '{valor}' é inválido.");
        mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        mensagens.SetUnknownValueIsInvalidAccessor(_ => "O valor informado é inválido.");
        mensagens.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado é inválido.");
        mensagens.SetValueMustBeANumberAccessor(_ => "O valor deve ser um número.");
        mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O valor deve ser um número.");
        mensagens.SetMissingBindRequiredValueAccessor(_ => "O campo é obrigatório.");
        mensagens.SetMissingKeyOrValueAccessor(() => "O campo é obrigatório.");
        mensagens.SetValueMustNotBeNullAccessor(_ => "O campo é obrigatório.");
        mensagens.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");
    }
}
