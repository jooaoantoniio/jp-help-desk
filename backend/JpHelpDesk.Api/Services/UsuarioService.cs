using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

public class UsuarioService(
    IUsuarioRepository repository,
    ISenhaHasher senhaHasher,
    TimeProvider timeProvider) : IUsuarioService
{
    public async Task<ResultadoPaginado<UsuarioResponse>> ListarAsync(UsuarioQuery query, CancellationToken cancellationToken)
    {
        var resultado = await repository.ListarAsync(query, cancellationToken);
        return resultado.Map(UsuarioResponse.FromEntity);
    }

    public async Task<UsuarioResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await ObterEntidadeAsync(id, cancellationToken);
        return UsuarioResponse.FromEntity(usuario);
    }

    public async Task<UsuarioResponse> CriarAsync(UsuarioCreateRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizarEmail(request.Email);
        await GarantirEmailUnicoAsync(email, ignorarId: null, cancellationToken);

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email,
            SenhaHash = senhaHasher.GerarHash(request.Senha),
            Perfil = request.Perfil!.Value, // [Required] garante que veio preenchido.
            Ativo = true,
            CriadoEm = timeProvider.GetUtcNow()
        };

        repository.Adicionar(usuario);
        await repository.SalvarAlteracoesAsync(cancellationToken);

        return UsuarioResponse.FromEntity(usuario);
    }

    public async Task<UsuarioResponse> AtualizarAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken)
    {
        var usuario = await ObterEntidadeAsync(id, cancellationToken);

        var email = NormalizarEmail(request.Email);
        await GarantirEmailUnicoAsync(email, ignorarId: id, cancellationToken);

        var novoPerfil = request.Perfil!.Value;
        if (usuario.Perfil == PerfilUsuario.Admin && novoPerfil != PerfilUsuario.Admin)
        {
            await GarantirOutroAdminAtivoAsync(usuario, cancellationToken);
        }

        usuario.Nome = request.Nome.Trim();
        usuario.Email = email;
        usuario.Perfil = novoPerfil;
        usuario.AtualizadoEm = timeProvider.GetUtcNow();

        await repository.SalvarAlteracoesAsync(cancellationToken);

        return UsuarioResponse.FromEntity(usuario);
    }

    public async Task DesativarAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await ObterEntidadeAsync(id, cancellationToken);

        if (!usuario.Ativo)
        {
            return; // Idempotente.
        }

        if (usuario.Perfil == PerfilUsuario.Admin)
        {
            await GarantirOutroAdminAtivoAsync(usuario, cancellationToken);
        }

        usuario.Ativo = false;
        usuario.AtualizadoEm = timeProvider.GetUtcNow();
        await repository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task AtivarAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await ObterEntidadeAsync(id, cancellationToken);

        if (usuario.Ativo)
        {
            return; // Idempotente.
        }

        usuario.Ativo = true;
        usuario.AtualizadoEm = timeProvider.GetUtcNow();
        await repository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Usuario> ObterEntidadeAsync(int id, CancellationToken cancellationToken) =>
        await repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNaoEncontradoException($"Usuário {id} não encontrado.");

    private async Task GarantirEmailUnicoAsync(string email, int? ignorarId, CancellationToken cancellationToken)
    {
        if (await repository.EmailExisteAsync(email, ignorarId, cancellationToken))
        {
            throw new ConflitoException($"O e-mail '{email}' já está em uso.");
        }
    }

    /// <summary>
    /// Impede que o sistema fique sem nenhum administrador ativo.
    /// </summary>
    private async Task GarantirOutroAdminAtivoAsync(Usuario admin, CancellationToken cancellationToken)
    {
        var adminsAtivos = await repository.ContarAtivosPorPerfilAsync(PerfilUsuario.Admin, cancellationToken);

        if (admin.Ativo && adminsAtivos <= 1)
        {
            throw new RegraNegocioException("O sistema precisa de pelo menos um administrador ativo.");
        }
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
