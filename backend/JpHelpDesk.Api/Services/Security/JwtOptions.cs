using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Configuração do JWT (seção "Jwt"). Validada na inicialização da aplicação.
/// </summary>
public class JwtOptions
{
    public const string Secao = "Jwt";

    /// <summary>Quem emite o token (esta API).</summary>
    [Required]
    public string Emissor { get; set; } = string.Empty;

    /// <summary>Para quem o token é destinado (o frontend).</summary>
    [Required]
    public string Audiencia { get; set; } = string.Empty;

    /// <summary>
    /// Chave de assinatura HMAC-SHA256. Segredo: vem de User Secrets / variável de ambiente, nunca do Git.
    /// </summary>
    [Required(ErrorMessage = "Configure 'Jwt:ChaveSecreta' via User Secrets ou variável de ambiente.")]
    [MinLength(32, ErrorMessage = "'Jwt:ChaveSecreta' deve ter no mínimo 32 caracteres (256 bits).")]
    public string ChaveSecreta { get; set; } = string.Empty;

    /// <summary>Tempo de validade do token, em minutos.</summary>
    [Range(5, 1440)]
    public int ExpiracaoMinutos { get; set; } = 60;
}
