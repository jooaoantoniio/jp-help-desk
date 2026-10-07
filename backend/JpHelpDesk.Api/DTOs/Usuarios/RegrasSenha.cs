namespace JpHelpDesk.Api.DTOs.Usuarios;

/// <summary>
/// Política de senha forte, compartilhada pelos DTOs que recebem senha.
/// </summary>
public static class RegrasSenha
{
    public const string Padrao = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$";

    public const string Mensagem =
        "A senha deve conter letra maiúscula, letra minúscula, número e caractere especial.";
}
