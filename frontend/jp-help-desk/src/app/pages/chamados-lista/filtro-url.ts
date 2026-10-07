import { ParamMap, Params } from '@angular/router';
import { ChamadoFiltro } from '../../models/chamado';
import { OrdenacaoChamado, PrioridadeChamado, StatusChamado } from '../../models/enums';

/**
 * Filtros da lista de chamados como ficam na URL (fonte da verdade da tela).
 * Datas no formato do <input type="date"> (AAAA-MM-DD).
 */
export interface FiltroTela {
  titulo: string;
  id: number | null;
  status: StatusChamado | null;
  prioridade: PrioridadeChamado | null;
  categoriaId: number | null;
  solicitanteId: number | null;
  tecnicoId: number | null;
  de: string;
  ate: string;
  ordenarPor: OrdenacaoChamado;
  decrescente: boolean;
  pagina: number;
  tamanhoPagina: number;
}

export const ORDENACAO_PADRAO: OrdenacaoChamado = 'DATA_ABERTURA';
export const TAMANHO_PAGINA_PADRAO = 10;

const inteiro = (valor: string | null): number | null => {
  const numero = Number(valor);
  return valor && Number.isInteger(numero) && numero > 0 ? numero : null;
};

/** Lê os filtros da URL, aplicando valores padrão para o que estiver ausente ou inválido. */
export function lerFiltroDaUrl(params: ParamMap): FiltroTela {
  return {
    titulo: params.get('titulo') ?? '',
    id: inteiro(params.get('id')),
    status: (params.get('status') as StatusChamado | null) || null,
    prioridade: (params.get('prioridade') as PrioridadeChamado | null) || null,
    categoriaId: inteiro(params.get('categoriaId')),
    solicitanteId: inteiro(params.get('solicitanteId')),
    tecnicoId: inteiro(params.get('tecnicoId')),
    de: params.get('de') ?? '',
    ate: params.get('ate') ?? '',
    ordenarPor: (params.get('ordenarPor') as OrdenacaoChamado | null) || ORDENACAO_PADRAO,
    decrescente: params.get('decrescente') !== 'false',
    pagina: inteiro(params.get('pagina')) ?? 1,
    tamanhoPagina: inteiro(params.get('tamanhoPagina')) ?? TAMANHO_PAGINA_PADRAO,
  };
}

/** Monta os query params da URL, omitindo vazios e valores padrão (URL curta e legível). */
export function filtroParaUrl(filtro: Partial<FiltroTela>): Params {
  const params: Params = {};
  const definir = (chave: string, valor: unknown) => {
    if (valor !== null && valor !== undefined && valor !== '') {
      params[chave] = valor;
    }
  };

  definir('titulo', filtro.titulo?.trim());
  definir('id', filtro.id);
  definir('status', filtro.status);
  definir('prioridade', filtro.prioridade);
  definir('categoriaId', filtro.categoriaId);
  definir('solicitanteId', filtro.solicitanteId);
  definir('tecnicoId', filtro.tecnicoId);
  definir('de', filtro.de);
  definir('ate', filtro.ate);
  if (filtro.ordenarPor && filtro.ordenarPor !== ORDENACAO_PADRAO) definir('ordenarPor', filtro.ordenarPor);
  if (filtro.decrescente === false) definir('decrescente', false);
  if (filtro.pagina && filtro.pagina > 1) definir('pagina', filtro.pagina);
  if (filtro.tamanhoPagina && filtro.tamanhoPagina !== TAMANHO_PAGINA_PADRAO) definir('tamanhoPagina', filtro.tamanhoPagina);

  return params;
}

/** Converte o filtro da tela no formato da API (datas viram ISO 8601 com o dia inteiro, no fuso local). */
export function filtroParaApi(filtro: FiltroTela): ChamadoFiltro {
  return {
    titulo: filtro.titulo || undefined,
    id: filtro.id ?? undefined,
    status: filtro.status ?? undefined,
    prioridade: filtro.prioridade ?? undefined,
    categoriaId: filtro.categoriaId ?? undefined,
    solicitanteId: filtro.solicitanteId ?? undefined,
    tecnicoId: filtro.tecnicoId ?? undefined,
    abertoDe: filtro.de ? new Date(`${filtro.de}T00:00:00`).toISOString() : undefined,
    abertoAte: filtro.ate ? new Date(`${filtro.ate}T23:59:59.999`).toISOString() : undefined,
    ordenarPor: filtro.ordenarPor,
    decrescente: filtro.decrescente,
    pagina: filtro.pagina,
    tamanhoPagina: filtro.tamanhoPagina,
  };
}

/** Quantidade de filtros ativos (exceto ordenação e paginação). */
export function contarFiltros(filtro: FiltroTela): number {
  return [filtro.titulo, filtro.id, filtro.status, filtro.prioridade, filtro.categoriaId,
    filtro.solicitanteId, filtro.tecnicoId, filtro.de, filtro.ate].filter((v) => v !== null && v !== '').length;
}
