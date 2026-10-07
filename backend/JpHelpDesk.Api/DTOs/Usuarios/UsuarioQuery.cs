using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Usuarios;

/// <summary>
/// Filtros da listagem de usuários.
/// </summary>
public class UsuarioQuery : PaginacaoQuery
{
    /// <summary>Texto contido no nome ou no e-mail.</summary>
    public string? Busca { get; set; }

    /// <summary>Filtra por perfil (ADMIN, TECNICO, USUARIO).</summary>
    public PerfilUsuario? Perfil { get; set; }

    /// <summary>Filtra por situação (true = ativos, false = inativos). Vazio retorna todos.</summary>
    public bool? Ativo { get; set; }
}
