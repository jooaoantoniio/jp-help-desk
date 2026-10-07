import { HttpParams } from '@angular/common/http';

type ValorParametro = string | number | boolean | null | undefined;

/** Converte um objeto de filtros em HttpParams, ignorando valores vazios (null, undefined ou ""). */
export function paraHttpParams(filtros: object): HttpParams {
  let params = new HttpParams();

  for (const [chave, valor] of Object.entries(filtros) as [string, ValorParametro][]) {
    if (valor !== null && valor !== undefined && valor !== '') {
      params = params.set(chave, String(valor));
    }
  }

  return params;
}
