// Enums da API, representados como union types de string (o mesmo texto do JSON).
// Os rótulos são o texto exibido ao usuário.

export type PerfilUsuario = 'ADMIN' | 'TECNICO' | 'USUARIO';

export type StatusChamado =
  | 'ABERTO'
  | 'EM_ATENDIMENTO'
  | 'AGUARDANDO_USUARIO'
  | 'RESOLVIDO'
  | 'FECHADO'
  | 'CANCELADO';

export type PrioridadeChamado = 'BAIXA' | 'MEDIA' | 'ALTA' | 'CRITICA';

export type TipoHistorico =
  | 'ABERTURA'
  | 'ATRIBUICAO'
  | 'ALTERACAO_STATUS'
  | 'ALTERACAO_DADOS'
  | 'COMENTARIO';

export type OrdenacaoChamado = 'DATA_ABERTURA' | 'PRIORIDADE' | 'STATUS' | 'TITULO' | 'ID';

export const PERFIL_LABEL: Record<PerfilUsuario, string> = {
  ADMIN: 'Administrador',
  TECNICO: 'Técnico',
  USUARIO: 'Usuário',
};

export const STATUS_LABEL: Record<StatusChamado, string> = {
  ABERTO: 'Aberto',
  EM_ATENDIMENTO: 'Em atendimento',
  AGUARDANDO_USUARIO: 'Aguardando usuário',
  RESOLVIDO: 'Resolvido',
  FECHADO: 'Fechado',
  CANCELADO: 'Cancelado',
};

export const PRIORIDADE_LABEL: Record<PrioridadeChamado, string> = {
  BAIXA: 'Baixa',
  MEDIA: 'Média',
  ALTA: 'Alta',
  CRITICA: 'Crítica',
};

export const TIPO_HISTORICO_LABEL: Record<TipoHistorico, string> = {
  ABERTURA: 'Abertura',
  ATRIBUICAO: 'Atribuição',
  ALTERACAO_STATUS: 'Status',
  ALTERACAO_DADOS: 'Edição',
  COMENTARIO: 'Comentário',
};

/** Classe CSS do badge (ex.: EM_ATENDIMENTO -> "status-em-atendimento"). */
export function classeBadge(prefixo: 'status' | 'prioridade' | 'perfil', valor: string): string {
  return `${prefixo}-${valor.toLowerCase().replaceAll('_', '-')}`;
}
