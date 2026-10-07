import { Chamado } from '../../models/chamado';
import { StatusChamado, TipoHistorico } from '../../models/enums';

export interface AcaoStatus {
  status: StatusChamado;
  rotulo: string;
  icone: string;
  /** Texto do diálogo de confirmação. */
  descricao: string;
  destrutiva: boolean;
  /** Motivo de a ação estar indisponível (ex.: falta técnico). */
  bloqueio: string | null;
}

/**
 * Botões de mudança de status a exibir. Usa "proximosStatus", que a API já calcula
 * respeitando o fluxo e o perfil do usuário — a regra não é duplicada no frontend.
 */
export function acoesDeStatus(chamado: Chamado): AcaoStatus[] {
  return chamado.proximosStatus.map((status) => {
    switch (status) {
      case 'EM_ATENDIMENTO':
        if (chamado.status === 'RESOLVIDO') {
          return acao(status, 'Reabrir chamado', 'replay', 'O problema voltou a acontecer? O chamado retornará para atendimento.');
        }
        return {
          ...acao(
            status,
            chamado.status === 'AGUARDANDO_USUARIO' ? 'Retomar atendimento' : 'Iniciar atendimento',
            'play_arrow',
            'O chamado passará para "Em atendimento".',
          ),
          bloqueio: chamado.tecnico ? null : 'Atribua um técnico antes de iniciar o atendimento.',
        };
      case 'AGUARDANDO_USUARIO':
        return acao(status, 'Aguardar usuário', 'hourglass_top', 'Use quando precisar de uma resposta ou ação do solicitante.');
      case 'RESOLVIDO':
        return acao(status, 'Marcar como resolvido', 'task_alt', 'Descreva a solução aplicada para que o solicitante possa confirmar.');
      case 'FECHADO':
        return acao(status, 'Confirmar e fechar', 'lock', 'Confirma que o problema foi solucionado. Chamados fechados não podem ser reabertos.');
      case 'CANCELADO':
        return { ...acao(status, 'Cancelar chamado', 'cancel', 'O chamado será cancelado e não poderá mais ser alterado.'), destrutiva: true };
      default:
        return acao(status, status, 'sync_alt', '');
    }
  });
}

function acao(status: StatusChamado, rotulo: string, icone: string, descricao: string): AcaoStatus {
  return { status, rotulo, icone, descricao, destrutiva: false, bloqueio: null };
}

/** Ícone de cada tipo de evento na linha do tempo. */
export const ICONE_HISTORICO: Record<TipoHistorico, string> = {
  ABERTURA: 'flag',
  ATRIBUICAO: 'person_add',
  ALTERACAO_STATUS: 'sync_alt',
  ALTERACAO_DADOS: 'edit',
  COMENTARIO: 'chat',
};

export const ehFinal = (status: StatusChamado) => status === 'FECHADO' || status === 'CANCELADO';
export const permiteAtribuicao = (status: StatusChamado) =>
  status === 'ABERTO' || status === 'EM_ATENDIMENTO' || status === 'AGUARDANDO_USUARIO';
