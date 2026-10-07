import { Paginacao } from './comum';

export interface Categoria {
  id: number;
  nome: string;
  descricao: string | null;
  ativo: boolean;
  criadoEm: string;
}

export interface CategoriaRequest {
  nome: string;
  descricao?: string | null;
}

export interface CategoriaFiltro extends Paginacao {
  busca?: string;
  ativo?: boolean;
}
