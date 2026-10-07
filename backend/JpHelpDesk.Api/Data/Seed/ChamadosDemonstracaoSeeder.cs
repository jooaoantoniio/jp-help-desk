using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Data.Seed;

/// <summary>
/// Gera chamados de demonstração com histórico coerente (abertura → atribuição → atendimento → solução),
/// distribuídos nos últimos 30 dias. Só executa se o banco não tiver nenhum chamado.
/// </summary>
public class ChamadosDemonstracaoSeeder(AppDbContext context, TimeProvider timeProvider, ILogger logger)
{
    private record Modelo(string Titulo, string Descricao, string Categoria, PrioridadeChamado Prioridade);

    // Do mais antigo para o mais recente.
    private static readonly Modelo[] Modelos =
    [
        new("Notebook não liga", "O notebook não dá sinal ao pressionar o botão de energia, mesmo na tomada.", "Hardware", PrioridadeChamado.Alta),
        new("Instalar Power BI Desktop", "Preciso do Power BI Desktop instalado para os relatórios do financeiro.", "Software", PrioridadeChamado.Baixa),
        new("Impressora do RH atolando papel", "A impressora do RH atola papel a cada duas ou três folhas impressas.", "Impressora", PrioridadeChamado.Media),
        new("Sem acesso à pasta do Comercial", "Ao abrir a pasta compartilhada do Comercial aparece \"acesso negado\".", "Acesso", PrioridadeChamado.Media),
        new("Outlook não sincroniza no celular", "Os e-mails pararam de chegar no aplicativo do celular desde ontem.", "E-mail", PrioridadeChamado.Baixa),
        new("Monitor piscando", "O monitor secundário pisca e apaga sozinho durante o uso.", "Hardware", PrioridadeChamado.Baixa),
        new("ERP lento no fechamento do mês", "O módulo de faturamento do ERP está demorando mais de um minuto para salvar notas.", "Sistema", PrioridadeChamado.Alta),
        new("Criar conta para novo colaborador", "Novo colaborador do Marketing começa segunda-feira e precisa de e-mail e acesso à rede.", "Acesso", PrioridadeChamado.Media),
        new("Wi-Fi caindo na sala de reunião", "A conexão Wi-Fi cai várias vezes durante as videoconferências na sala 3.", "Rede", PrioridadeChamado.Media),
        new("Atualizar Windows da recepção", "O computador da recepção mostra aviso de atualização pendente há semanas.", "Software", PrioridadeChamado.Baixa),
        new("Teclado com teclas falhando", "As teclas A e S do teclado não funcionam na primeira tentativa.", "Hardware", PrioridadeChamado.Baixa),
        new("Licença do Office expirada", "O Word abre em modo somente leitura informando que a licença expirou.", "Software", PrioridadeChamado.Media),
        new("Não consigo imprimir em cores", "A impressora colorida do 2º andar só imprime em preto e branco.", "Impressora", PrioridadeChamado.Baixa),
        new("VPN não conecta em home office", "A VPN retorna erro de autenticação ao tentar conectar de casa.", "Rede", PrioridadeChamado.Alta),
        new("Caixa de e-mail cheia", "Recebi aviso de que a caixa de e-mail atingiu o limite e não recebe novas mensagens.", "E-mail", PrioridadeChamado.Media),
        new("Servidor de arquivos fora do ar", "Ninguém do setor consegue acessar o servidor de arquivos (unidade S:).", "Rede", PrioridadeChamado.Critica),
        new("Erro ao emitir nota fiscal no ERP", "O ERP mostra \"falha de comunicação com a SEFAZ\" ao emitir notas.", "Sistema", PrioridadeChamado.Critica),
        new("Solicitar segundo monitor", "Gostaria de um segundo monitor para trabalhar com planilhas grandes.", "Hardware", PrioridadeChamado.Baixa),
        new("Senha do sistema bloqueada", "Errei a senha do sistema de RH e o usuário foi bloqueado.", "Acesso", PrioridadeChamado.Alta),
        new("Internet lenta no financeiro", "Downloads e sistemas web estão muito lentos em todo o setor financeiro.", "Rede", PrioridadeChamado.Alta),
        new("Scanner não envia para o e-mail", "O scanner da impressora não está mais enviando os documentos digitalizados por e-mail.", "Impressora", PrioridadeChamado.Media),
        new("Sistema de ponto fora do ar", "O sistema de registro de ponto não abre para nenhum colaborador.", "Sistema", PrioridadeChamado.Critica),
        new("Instalar antivírus no notebook novo", "O notebook recebido esta semana ainda não tem o antivírus corporativo.", "Software", PrioridadeChamado.Media),
        new("Dúvida sobre backup dos arquivos", "Gostaria de saber se os arquivos da minha área de trabalho entram no backup.", "Outros", PrioridadeChamado.Baixa)
    ];

    // Status final de cada modelo (mesma ordem): os mais antigos já estão concluídos, os recentes em aberto.
    private static readonly StatusChamado[] StatusFinais =
    [
        StatusChamado.Fechado, StatusChamado.Fechado, StatusChamado.Fechado, StatusChamado.Fechado, StatusChamado.Fechado,
        StatusChamado.Cancelado,
        StatusChamado.Resolvido, StatusChamado.Resolvido, StatusChamado.Resolvido, StatusChamado.Resolvido,
        StatusChamado.Cancelado,
        StatusChamado.AguardandoUsuario, StatusChamado.AguardandoUsuario, StatusChamado.AguardandoUsuario,
        StatusChamado.EmAtendimento, StatusChamado.EmAtendimento, StatusChamado.EmAtendimento,
        StatusChamado.EmAtendimento, StatusChamado.EmAtendimento,
        StatusChamado.Aberto, StatusChamado.Aberto, StatusChamado.Aberto, StatusChamado.Aberto, StatusChamado.Aberto
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Chamados.AnyAsync(cancellationToken))
        {
            return;
        }

        var usuarios = await context.Usuarios.ToDictionaryAsync(u => u.Email, cancellationToken);
        var categorias = await context.Categorias.ToDictionaryAsync(c => c.Nome, cancellationToken);
        var admin = usuarios["admin@jphelpdesk.com"];
        var tecnico = usuarios["tecnico@jphelpdesk.com"];
        Usuario[] solicitantes =
        [
            usuarios["usuario@jphelpdesk.com"],
            usuarios["maria.oliveira@jphelpdesk.com"],
            usuarios["carlos.lima@jphelpdesk.com"]
        ];

        var aleatorio = new Random(2026); // semente fixa: os mesmos dados a cada execução
        var agora = timeProvider.GetUtcNow();

        for (var i = 0; i < Modelos.Length; i++)
        {
            var diasAtras = 29 - (i * 29.0 / (Modelos.Length - 1));
            var abertura = agora.AddDays(-diasAtras).AddHours(-aleatorio.Next(1, 8));
            var chamado = CriarChamado(Modelos[i], StatusFinais[i], solicitantes[i % solicitantes.Length],
                tecnico, admin, categorias[Modelos[i].Categoria], abertura, agora, aleatorio);
            context.Chamados.Add(chamado);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Chamados de demonstração criados: {Quantidade}", Modelos.Length);
    }

    /// <summary>Monta o chamado percorrendo o fluxo real de status até o status final desejado.</summary>
    private static Chamado CriarChamado(
        Modelo modelo, StatusChamado statusFinal, Usuario solicitante, Usuario tecnico, Usuario admin,
        Categoria categoria, DateTimeOffset abertura, DateTimeOffset agora, Random aleatorio)
    {
        var chamado = new Chamado
        {
            Titulo = modelo.Titulo,
            Descricao = modelo.Descricao,
            Prioridade = modelo.Prioridade,
            Status = StatusChamado.Aberto,
            Categoria = categoria,
            Solicitante = solicitante,
            DataAbertura = abertura
        };

        var momento = abertura;
        DateTimeOffset Avancar() => momento = Min(momento.AddMinutes(aleatorio.Next(15, 180)), agora);

        void Registrar(TipoHistorico tipo, Usuario usuario, string descricao) =>
            chamado.Historicos.Add(new HistoricoChamado { Usuario = usuario, Tipo = tipo, Descricao = descricao, DataRegistro = momento });

        void Mudar(StatusChamado novo, Usuario usuario, string? observacao = null)
        {
            Avancar();
            var descricao = $"Status alterado de {chamado.Status.ToApiString()} para {novo.ToApiString()}.";
            Registrar(TipoHistorico.AlteracaoStatus, usuario, observacao is null ? descricao : $"{descricao} Observação: {observacao}");
            chamado.Status = novo;
            chamado.DataAtualizacao = momento;
            chamado.DataFechamento = FluxoStatusChamado.EhFinal(novo) ? momento : null;
        }

        Registrar(TipoHistorico.Abertura, solicitante,
            $"Chamado aberto por {solicitante.Nome} com prioridade {modelo.Prioridade.ToApiString()}.");

        if (statusFinal == StatusChamado.Cancelado)
        {
            Mudar(StatusChamado.Cancelado, solicitante);
            return chamado;
        }

        if (statusFinal == StatusChamado.Aberto)
        {
            return chamado;
        }

        Avancar();
        chamado.Tecnico = tecnico;
        chamado.DataAtualizacao = momento;
        Registrar(TipoHistorico.Atribuicao, admin, $"Chamado atribuído ao técnico {tecnico.Nome}.");

        Mudar(StatusChamado.EmAtendimento, tecnico);

        if (statusFinal == StatusChamado.AguardandoUsuario)
        {
            Avancar();
            Registrar(TipoHistorico.Comentario, tecnico, "Poderia informar quando posso acessar seu computador remotamente?");
            Mudar(StatusChamado.AguardandoUsuario, tecnico);
        }
        else if (statusFinal is StatusChamado.Resolvido or StatusChamado.Fechado)
        {
            Mudar(StatusChamado.Resolvido, tecnico, "Problema corrigido e testado com o usuário.");

            if (statusFinal == StatusChamado.Fechado)
            {
                Mudar(StatusChamado.Fechado, solicitante);
            }
        }

        return chamado;
    }

    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
