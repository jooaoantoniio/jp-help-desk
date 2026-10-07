using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;

namespace JpHelpDesk.Api.Services;

/// <summary>
/// Regras de negócio dos chamados. Toda alteração gera um registro de histórico,
/// salvo na mesma transação (um único SaveChanges) que a própria alteração.
/// Permissões: ADMIN/TECNICO atuam em qualquer chamado; USUARIO só nos que abriu
/// (chamados de terceiros respondem 404, para não revelar que existem).
/// </summary>
public class ChamadoService(
    IChamadoRepository chamadoRepository,
    ICategoriaRepository categoriaRepository,
    IUsuarioRepository usuarioRepository,
    IUsuarioAtual usuarioAtual,
    TimeProvider timeProvider) : IChamadoService
{
    public async Task<ResultadoPaginado<ChamadoResponse>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);

        // USUARIO só enxerga os próprios chamados, independentemente do filtro enviado.
        if (!PermissoesChamado.EhEquipe(usuario))
        {
            query.SolicitanteId = usuario.Id;
        }

        var resultado = await chamadoRepository.ListarAsync(query, cancellationToken);
        return resultado.Map(chamado => ChamadoResponse.FromEntity(chamado, usuario));
    }

    public async Task<ChamadoResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);
        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task<ChamadoResponse> AbrirAsync(ChamadoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var categoria = await ObterCategoriaAtivaAsync(request.CategoriaId!.Value, cancellationToken);
        var solicitante = await usuarioRepository.ObterPorIdAsync(usuario.Id, cancellationToken)
            ?? throw new NaoAutenticadoException("Usuário não encontrado.");
        var agora = timeProvider.GetUtcNow();

        var chamado = new Chamado
        {
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            Prioridade = request.Prioridade!.Value,
            Status = StatusChamado.Aberto,
            Categoria = categoria,
            Solicitante = solicitante,
            DataAbertura = agora
        };

        RegistrarHistorico(chamado, usuario, TipoHistorico.Abertura,
            $"Chamado aberto por {usuario.Nome} com prioridade {chamado.Prioridade.ToApiString()}.", agora);

        chamadoRepository.Adicionar(chamado);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task<ChamadoResponse> AtualizarAsync(int id, ChamadoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);
        GarantirNaoFinalizado(chamado, "editados");

        if (!PermissoesChamado.PodeEditar(chamado, usuario))
        {
            throw new AcessoNegadoException("Você só pode editar seus chamados enquanto estiverem com status ABERTO.");
        }

        var titulo = request.Titulo.Trim();
        var descricao = request.Descricao.Trim();
        var prioridade = request.Prioridade!.Value;
        var alteracoes = new List<string>();

        if (chamado.Titulo != titulo)
        {
            alteracoes.Add("título");
            chamado.Titulo = titulo;
        }

        if (chamado.Descricao != descricao)
        {
            alteracoes.Add("descrição");
            chamado.Descricao = descricao;
        }

        if (chamado.Prioridade != prioridade)
        {
            alteracoes.Add($"prioridade ({chamado.Prioridade.ToApiString()} → {prioridade.ToApiString()})");
            chamado.Prioridade = prioridade;
        }

        // Só exige categoria ativa se ela estiver sendo trocada.
        if (chamado.CategoriaId != request.CategoriaId)
        {
            var novaCategoria = await ObterCategoriaAtivaAsync(request.CategoriaId!.Value, cancellationToken);
            alteracoes.Add($"categoria ({chamado.Categoria.Nome} → {novaCategoria.Nome})");
            chamado.Categoria = novaCategoria;
        }

        if (alteracoes.Count == 0)
        {
            return ChamadoResponse.FromEntity(chamado, usuario); // Nada mudou: sem histórico.
        }

        var agora = timeProvider.GetUtcNow();
        chamado.DataAtualizacao = agora;
        RegistrarHistorico(chamado, usuario, TipoHistorico.AlteracaoDados,
            $"Dados alterados: {string.Join(", ", alteracoes)}.", agora);

        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task<ChamadoResponse> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);

        AplicarStatus(chamado, request.Status!.Value, usuario, request.Observacao);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task<ChamadoResponse> AtribuirTecnicoAsync(int id, AtribuirTecnicoRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);

        await AtribuirAsync(chamado, request.TecnicoId!.Value, usuario, cancellationToken);

        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task<ChamadoResponse> AssumirAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);

        // Defesa em profundidade: o controller já restringe à equipe.
        if (!PermissoesChamado.EhEquipe(usuario))
        {
            throw new AcessoNegadoException("Somente técnicos e administradores podem assumir chamados.");
        }

        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);
        await AtribuirAsync(chamado, usuario.Id, usuario, cancellationToken);

        return ChamadoResponse.FromEntity(chamado, usuario);
    }

    public async Task CancelarAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);

        if (chamado.Status == StatusChamado.Cancelado)
        {
            return; // Idempotente.
        }

        AplicarStatus(chamado, StatusChamado.Cancelado, usuario, observacao: null);
        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    public async Task<HistoricoResponse> ComentarAsync(int id, ComentarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        var chamado = await ObterVisivelAsync(id, usuario, cancellationToken);
        GarantirNaoFinalizado(chamado, "comentados");

        var agora = timeProvider.GetUtcNow();
        chamado.DataAtualizacao = agora;
        var historico = RegistrarHistorico(chamado, usuario, TipoHistorico.Comentario, request.Texto.Trim(), agora);

        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);

        return new HistoricoResponse(
            historico.Id,
            historico.Tipo,
            historico.Descricao,
            new ReferenciaResponse(usuario.Id, usuario.Nome),
            historico.DataRegistro);
    }

    public async Task<IReadOnlyList<HistoricoResponse>> ListarHistoricoAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioAtual.ObterAsync(cancellationToken);
        await ObterVisivelAsync(id, usuario, cancellationToken);

        var historico = await chamadoRepository.ListarHistoricoAsync(id, cancellationToken);
        return historico.Select(HistoricoResponse.FromEntity).ToList();
    }

    /// <summary>
    /// Atribui o técnico ao chamado (usado por "atribuir" e por "assumir") e registra o histórico.
    /// </summary>
    private async Task AtribuirAsync(Chamado chamado, int tecnicoId, UsuarioLogado usuario, CancellationToken cancellationToken)
    {
        if (!FluxoStatusChamado.PermiteAtribuicao(chamado.Status))
        {
            throw new RegraNegocioException(
                $"Chamados com status {chamado.Status.ToApiString()} não podem ser atribuídos.");
        }

        if (chamado.TecnicoId == tecnicoId)
        {
            return; // Já atribuído a este técnico.
        }

        var tecnico = await usuarioRepository.ObterPorIdAsync(tecnicoId, cancellationToken);

        if (tecnico is null || !tecnico.Ativo || tecnico.Perfil == PerfilUsuario.Usuario)
        {
            throw new RegraNegocioException(
                $"Usuário {tecnicoId} não encontrado, inativo ou sem perfil TECNICO/ADMIN.");
        }

        var descricao = (chamado.Tecnico, tecnico.Id == usuario.Id) switch
        {
            (null, true) => $"Chamado assumido por {tecnico.Nome}.",
            (null, false) => $"Chamado atribuído ao técnico {tecnico.Nome}.",
            (_, true) => $"Chamado assumido por {tecnico.Nome} (antes com {chamado.Tecnico.Nome}).",
            (_, false) => $"Técnico responsável alterado de {chamado.Tecnico.Nome} para {tecnico.Nome}."
        };

        var agora = timeProvider.GetUtcNow();
        chamado.Tecnico = tecnico;
        chamado.DataAtualizacao = agora;
        RegistrarHistorico(chamado, usuario, TipoHistorico.Atribuicao, descricao, agora);

        await chamadoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>
    /// Valida permissão e transição na máquina de estados, atualiza status e datas e registra o histórico.
    /// </summary>
    private void AplicarStatus(Chamado chamado, StatusChamado novoStatus, UsuarioLogado usuario, string? observacao)
    {
        var statusAnterior = chamado.Status;

        // 1º o fluxo (transição inválida para qualquer perfil → 400); 2º a permissão do usuário (→ 403).
        if (!FluxoStatusChamado.PodeAlterar(statusAnterior, novoStatus))
        {
            var permitidos = FluxoStatusChamado.ProximosStatus(statusAnterior);
            var opcoes = permitidos.Count == 0
                ? "o status atual é final"
                : $"permitidos: {string.Join(", ", permitidos.Select(s => s.ToApiString()))}";

            throw new RegraNegocioException(
                $"Não é possível alterar o status de {statusAnterior.ToApiString()} para {novoStatus.ToApiString()} ({opcoes}).");
        }

        if (!PermissoesChamado.PodeAlterarStatus(chamado, usuario, novoStatus))
        {
            throw new AcessoNegadoException(
                "Você pode apenas cancelar seu chamado enquanto ABERTO, ou fechar/reabrir após RESOLVIDO.");
        }

        if (novoStatus == StatusChamado.EmAtendimento && chamado.TecnicoId is null)
        {
            throw new RegraNegocioException("Atribua um técnico antes de iniciar o atendimento.");
        }

        var agora = timeProvider.GetUtcNow();
        chamado.Status = novoStatus;
        chamado.DataAtualizacao = agora;
        chamado.DataFechamento = FluxoStatusChamado.EhFinal(novoStatus) ? agora : null;

        var descricao = $"Status alterado de {statusAnterior.ToApiString()} para {novoStatus.ToApiString()}.";
        if (statusAnterior == StatusChamado.Resolvido && novoStatus == StatusChamado.EmAtendimento)
        {
            descricao = $"Chamado reaberto. {descricao}";
        }
        if (!string.IsNullOrWhiteSpace(observacao))
        {
            descricao += $" Observação: {observacao.Trim()}";
        }

        RegistrarHistorico(chamado, usuario, TipoHistorico.AlteracaoStatus, descricao, agora);
    }

    private static HistoricoChamado RegistrarHistorico(
        Chamado chamado,
        UsuarioLogado usuario,
        TipoHistorico tipo,
        string descricao,
        DateTimeOffset dataRegistro)
    {
        var historico = new HistoricoChamado
        {
            UsuarioId = usuario.Id,
            Tipo = tipo,
            Descricao = descricao,
            DataRegistro = dataRegistro
        };

        // Adicionado pela navegação: o EF preenche o ChamadoId (inclusive de chamados novos).
        chamado.Historicos.Add(historico);
        return historico;
    }

    /// <summary>
    /// Carrega o chamado se o usuário puder vê-lo; caso contrário, responde como inexistente (404).
    /// </summary>
    private async Task<Chamado> ObterVisivelAsync(int id, UsuarioLogado usuario, CancellationToken cancellationToken)
    {
        var chamado = await chamadoRepository.ObterPorIdAsync(id, cancellationToken);

        if (chamado is null || !PermissoesChamado.PodeVisualizar(chamado, usuario))
        {
            throw new RecursoNaoEncontradoException($"Chamado {id} não encontrado.");
        }

        return chamado;
    }

    private async Task<Categoria> ObterCategoriaAtivaAsync(int categoriaId, CancellationToken cancellationToken)
    {
        var categoria = await categoriaRepository.ObterPorIdAsync(categoriaId, cancellationToken);

        if (categoria is null || !categoria.Ativo)
        {
            throw new RegraNegocioException($"Categoria {categoriaId} não encontrada ou inativa.");
        }

        return categoria;
    }

    private static void GarantirNaoFinalizado(Chamado chamado, string acao)
    {
        if (FluxoStatusChamado.EhFinal(chamado.Status))
        {
            throw new RegraNegocioException(
                $"Chamados com status {chamado.Status.ToApiString()} não podem ser {acao}.");
        }
    }
}
