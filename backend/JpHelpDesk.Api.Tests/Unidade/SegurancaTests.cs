using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Services.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace JpHelpDesk.Api.Tests.Unidade;

public class BCryptSenhaHasherTests
{
    private readonly BCryptSenhaHasher _hasher = new();

    [Fact]
    public void Hash_nao_contem_a_senha_e_valida_somente_a_senha_correta()
    {
        var hash = _hasher.GerarHash("Senha@Forte1");

        Assert.DoesNotContain("Senha@Forte1", hash);
        Assert.True(_hasher.Verificar("Senha@Forte1", hash));
        Assert.False(_hasher.Verificar("senha@forte1", hash));
    }

    [Fact]
    public void Mesma_senha_gera_hashes_diferentes_por_causa_do_salt()
    {
        Assert.NotEqual(_hasher.GerarHash("Senha@Forte1"), _hasher.GerarHash("Senha@Forte1"));
    }
}

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    private static JwtTokenService CriarServico() => new(
        Options.Create(new JwtOptions
        {
            Emissor = "emissor-teste",
            Audiencia = "audiencia-teste",
            ChaveSecreta = new string('k', 64),
            ExpiracaoMinutos = 30
        }),
        new RelogioFixo(Agora));

    [Fact]
    public void Token_traz_identificacao_perfil_e_validade_configurada()
    {
        var usuario = new Usuario { Id = 7, Nome = "Maria", Email = "maria@teste.com", Perfil = PerfilUsuario.Tecnico };

        var gerado = CriarServico().GerarToken(usuario);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(gerado.Token);

        Assert.Equal("7", token.Subject);
        Assert.Equal("TECNICO", token.GetClaim(JwtTokenService.Claims.Perfil).Value);
        Assert.Equal("maria@teste.com", token.GetClaim(JwtTokenService.Claims.Email).Value);
        Assert.Equal("emissor-teste", token.Issuer);
        Assert.Equal(Agora.AddMinutes(30), gerado.ExpiraEm);
        Assert.Equal(Agora.AddMinutes(30).UtcDateTime, token.ValidTo);
    }

    [Fact]
    public void Token_nao_carrega_dados_sensiveis()
    {
        var usuario = new Usuario { Id = 1, Nome = "A", Email = "a@a.com", Perfil = PerfilUsuario.Admin, SenhaHash = "hash-secreto" };

        var token = new JsonWebTokenHandler().ReadJsonWebToken(CriarServico().GerarToken(usuario).Token);

        Assert.DoesNotContain(token.Claims, c => c.Value.Contains("hash-secreto"));
    }

    [Fact]
    public void Cada_token_tem_identificador_unico()
    {
        var usuario = new Usuario { Id = 1, Nome = "A", Email = "a@a.com", Perfil = PerfilUsuario.Admin };
        var servico = CriarServico();
        var handler = new JsonWebTokenHandler();

        var jti1 = handler.ReadJsonWebToken(servico.GerarToken(usuario).Token).Id;
        var jti2 = handler.ReadJsonWebToken(servico.GerarToken(usuario).Token).Id;

        Assert.NotEqual(jti1, jti2);
    }
}
