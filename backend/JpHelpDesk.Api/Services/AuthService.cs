using JpHelpDesk.Api.DTOs.Auth;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

public class AuthService(
    IUsuarioRepository usuarioRepository,
    ISenhaHasher senhaHasher,
    ITokenService tokenService,
    IUsuarioAtual usuarioAtual,
    TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    // Mesma mensagem para e-mail inexistente, senha errada ou usuário inativo:
    // não revelamos a um atacante quais e-mails estão cadastrados.
    private const string CredenciaisInvalidas = "E-mail ou senha inválidos.";

    // Hash de uma senha aleatória, usado quando o e-mail não existe (ver LoginAsync).
    private static string? _hashFicticio;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await usuarioRepository.ObterPorEmailAsync(email, cancellationToken);

        // Sempre executa a verificação BCrypt (mesmo sem usuário), para o tempo de resposta
        // não denunciar se o e-mail existe (proteção contra "timing attack").
        var hash = usuario?.SenhaHash
            ?? LazyInitializer.EnsureInitialized(ref _hashFicticio, () => senhaHasher.GerarHash(Guid.NewGuid().ToString()));
        var senhaCorreta = senhaHasher.Verificar(request.Senha, hash);

        if (usuario is null || !senhaCorreta || !usuario.Ativo)
        {
            logger.LogWarning("Falha de login para {Email}", email);
            throw new NaoAutenticadoException(CredenciaisInvalidas);
        }

        var token = tokenService.GerarToken(usuario);
        logger.LogInformation("Login realizado: {Email} ({Perfil})", usuario.Email, usuario.Perfil);

        return new LoginResponse(token.Token, "Bearer", token.ExpiraEm, UsuarioResponse.FromEntity(usuario));
    }

    public async Task<UsuarioResponse> ObterPerfilAsync(CancellationToken cancellationToken)
    {
        var usuario = await ObterUsuarioLogadoAsync(cancellationToken);
        return UsuarioResponse.FromEntity(usuario);
    }

    public async Task AlterarSenhaAsync(AlterarSenhaRequest request, CancellationToken cancellationToken)
    {
        var usuario = await ObterUsuarioLogadoAsync(cancellationToken);

        if (!senhaHasher.Verificar(request.SenhaAtual, usuario.SenhaHash))
        {
            throw new RegraNegocioException("A senha atual está incorreta.");
        }

        if (request.SenhaAtual == request.NovaSenha)
        {
            throw new RegraNegocioException("A nova senha deve ser diferente da senha atual.");
        }

        usuario.SenhaHash = senhaHasher.GerarHash(request.NovaSenha);
        usuario.AtualizadoEm = timeProvider.GetUtcNow();
        await usuarioRepository.SalvarAlteracoesAsync(cancellationToken);

        logger.LogInformation("Senha alterada pelo usuário {UsuarioId}", usuario.Id);
    }

    private async Task<Usuario> ObterUsuarioLogadoAsync(CancellationToken cancellationToken)
    {
        var logado = await usuarioAtual.ObterAsync(cancellationToken);
        return await usuarioRepository.ObterPorIdAsync(logado.Id, cancellationToken)
            ?? throw new NaoAutenticadoException("Usuário não encontrado.");
    }
}
