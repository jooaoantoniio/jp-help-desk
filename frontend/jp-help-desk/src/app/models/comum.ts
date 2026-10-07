/** Página de resultados de uma listagem (ResultadoPaginado<T> da API). */
export interface ResultadoPaginado<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
}

/** Parâmetros de paginação aceitos pelas listagens. */
export interface Paginacao {
  pagina?: number;
  tamanhoPagina?: number;
}

/** Referência resumida (ID + nome) dentro de outras respostas. */
export interface Referencia {
  id: number;
  nome: string;
}

/** Erro no formato ProblemDetails (RFC 9457) retornado pela API. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  /** Presente em erros de validação (400): campo -> mensagens. */
  errors?: Record<string, string[]>;
}
