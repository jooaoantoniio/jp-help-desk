import { Chamado } from './chamado';
import { Referencia } from './comum';
import { PrioridadeChamado, StatusChamado } from './enums';

/** Resposta de GET /api/dashboard. */
export interface Dashboard {
  resumo: ResumoDashboard;
  porStatus: { status: StatusChamado; quantidade: number }[];
  porPrioridade: { prioridade: PrioridadeChamado; quantidade: number }[];
  porCategoria: { categoria: Referencia; quantidade: number }[];
  recentes: Chamado[];
}

export interface ResumoDashboard {
  total: number;
  abertos: number;
  emAtendimento: number;
  aguardandoUsuario: number;
  /** Prioridade CRITICA ainda não resolvida. */
  criticosEmAberto: number;
  /** RESOLVIDO + FECHADO. */
  resolvidos: number;
}
