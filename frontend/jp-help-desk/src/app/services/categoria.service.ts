import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Categoria, CategoriaFiltro, CategoriaRequest } from '../models/categoria';
import { ResultadoPaginado } from '../models/comum';
import { paraHttpParams } from './http-params';

/** Acesso à API de categorias (/api/categorias). */
@Injectable({ providedIn: 'root' })
export class CategoriaService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/categorias`;

  listar(filtro: CategoriaFiltro = {}): Observable<ResultadoPaginado<Categoria>> {
    return this.http.get<ResultadoPaginado<Categoria>>(this.url, { params: paraHttpParams(filtro) });
  }

  /** Categorias ativas (para selects de formulário e filtros). */
  listarAtivas(): Observable<Categoria[]> {
    return this.listar({ ativo: true, tamanhoPagina: 50 }).pipe(map((pagina) => pagina.itens));
  }

  criar(dados: CategoriaRequest): Observable<Categoria> {
    return this.http.post<Categoria>(this.url, dados);
  }

  atualizar(id: number, dados: CategoriaRequest): Observable<Categoria> {
    return this.http.put<Categoria>(`${this.url}/${id}`, dados);
  }

  desativar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  ativar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/ativar`, null);
  }
}
