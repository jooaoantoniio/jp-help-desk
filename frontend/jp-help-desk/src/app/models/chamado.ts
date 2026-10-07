import { Paginacao, Referencia } from './comum';
import { OrdenacaoChamado, PrioridadeChamado, StatusChamado, TipoHistorico } from './enums';

export interface Chamado {
  id: number;
  titulo: string;
  descricao: string;
  status: StatusChamado;
  prioridade: PrioridadeChamado;
  categoria: Referencia;
  solicitante: Referencia;
  tecnico: Referencia | null;
  dataAbertura: string;
  dataAtualizacao: string | null;
  dataFechamento: string | null;
  /** Status para os quais o usuário atual pode alterar o chamado. */
  proximosStatus: StatusChamado[];
}

export interface ChamadoRequest {
  titulo: string;
  descricao: string;
  prioridade: PrioridadeChamado;
  categoriaId: number;
}

export interface AlterarStatusRequest {
  status: StatusChamado;
  observacao?: string | null;
}

export interface Historico {
  id: number;
  tipo: TipoHistorico;
  descricao: string;
  usuario: Referencia;
  dataRegistro: string;
}

export interface ChamadoFiltro extends Paginacao {
  id?: number;
  titulo?: string;
  status?: StatusChamado;
  prioridade?: PrioridadeChamado;
  categoriaId?: number;
  solicitanteId?: number;
  tecnicoId?: number;
  /** ISO 8601 (ex.: 2026-10-01T00:00:00-03:00). */
  abertoDe?: string;
  abertoAte?: string;
  ordenarPor?: OrdenacaoChamado;
  decrescente?: boolean;
}
