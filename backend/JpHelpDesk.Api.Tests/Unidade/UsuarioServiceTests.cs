using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Tests.Unidade;

public class UsuarioServiceTests
{
    private readonly UsuarioRepositoryEmMemoria _repositorio = new();
    private readonly UsuarioService _servico;

    public UsuarioServiceTests()
    {
        _servico = new UsuarioService(_repositorio, new HasherFalso(), new RelogioFixo(DateTimeOffset.UnixEpoch));
    }

    private Usuario Adicionar(PerfilUsuario perfil, bool ativo = true)
    {
        var usuario = new Usuario { Nome = $"Usuário {perfil}", Email = $"{Guid.NewGuid():N}@teste.com", Perfil = perfil, Ativo = ativo };
        _repositorio.Adicionar(usuario);
        return usuario;
    }

    [Fact]
    public async Task Nao_desativa_o_ultimo_administrador_ativo()
    {
        var admin = Adicionar(PerfilUsuario.Admin);
        Adicionar(PerfilUsuario.Admin, ativo: false); // inativo não conta

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(() => _servico.DesativarAsync(admin.Id, CancellationToken.None));

        Assert.Equal("O sistema precisa de pelo menos um administrador ativo.", erro.Message);
        Assert.True(admin.Ativo);
    }

    [Fact]
    public async Task Desativa_administrador_quando_existe_outro_ativo()
    {
        var admin = Adicionar(PerfilUsuario.Admin);
        Adicionar(PerfilUsuario.Admin);

        await _servico.DesativarAsync(admin.Id, CancellationToken.None);

        Assert.False(admin.Ativo);
    }

    [Fact]
    public async Task Nao_rebaixa_o_ultimo_administrador()
    {
        var admin = Adicionar(PerfilUsuario.Admin);
        var dados = new UsuarioUpdateRequest { Nome = admin.Nome, Email = admin.Email, Perfil = PerfilUsuario.Tecnico };

        await Assert.ThrowsAsync<RegraNegocioException>(() => _servico.AtualizarAsync(admin.Id, dados, CancellationToken.None));
        Assert.Equal(PerfilUsuario.Admin, admin.Perfil);
    }

    [Fact]
    public async Task Cria_usuario_com_email_normalizado_e_senha_em_hash()
    {
        var criado = await _servico.CriarAsync(new UsuarioCreateRequest
        {
            Nome = "  Maria Souza ",
            Email = "  Maria.Souza@Empresa.COM ",
            Senha = "Senha@Forte1",
            Perfil = PerfilUsuario.Usuario
        }, CancellationToken.None);

        Assert.Equal("Maria Souza", criado.Nome);
        Assert.Equal("maria.souza@empresa.com", criado.Email);
        var salvo = _repositorio.Usuarios.Single();
        Assert.Equal("hash:Senha@Forte1", salvo.SenhaHash);
        Assert.True(salvo.Ativo);
    }

    [Fact]
    public async Task Email_duplicado_ignora_maiusculas_e_gera_conflito()
    {
        var existente = Adicionar(PerfilUsuario.Usuario);
        var dados = new UsuarioCreateRequest { Nome = "Outro", Email = existente.Email.ToUpperInvariant(), Senha = "Senha@Forte1", Perfil = PerfilUsuario.Usuario };

        await Assert.ThrowsAsync<ConflitoException>(() => _servico.CriarAsync(dados, CancellationToken.None));
    }

    [Fact]
    public async Task Usuario_inexistente_gera_404()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() => _servico.ObterPorIdAsync(999, CancellationToken.None));
    }

    /// <summary>Hash previsível: o teste verifica que o serviço não grava a senha em texto puro.</summary>
    private sealed class HasherFalso : ISenhaHasher
    {
        public string GerarHash(string senha) => $"hash:{senha}";
        public bool Verificar(string senha, string hash) => hash == $"hash:{senha}";
    }

    /// <summary>Implementação em memória de IUsuarioRepository, suficiente para as regras do serviço.</summary>
    private sealed class UsuarioRepositoryEmMemoria : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = [];

        public void Adicionar(Usuario usuario)
        {
            usuario.Id = Usuarios.Count + 1;
            Usuarios.Add(usuario);
        }

        public Task<Usuario?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id));

        public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.Email == email));

        public Task<bool> EmailExisteAsync(string email, int? ignorarId, CancellationToken cancellationToken) =>
            Task.FromResult(Usuarios.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && u.Id != ignorarId));

        public Task<int> ContarAtivosPorPerfilAsync(PerfilUsuario perfil, CancellationToken cancellationToken) =>
            Task.FromResult(Usuarios.Count(u => u.Ativo && u.Perfil == perfil));

        public Task<PerfilUsuario?> ObterPerfilSeAtivoAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == id && u.Ativo)?.Perfil);

        public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ResultadoPaginado<Usuario>> ListarAsync(UsuarioQuery query, CancellationToken cancellationToken) =>
            throw new NotSupportedException("Não usado nestes testes.");
    }
}
