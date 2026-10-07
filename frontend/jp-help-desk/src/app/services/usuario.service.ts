import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ResultadoPaginado } from '../models/comum';
import { Usuario, UsuarioCreateRequest, UsuarioFiltro, UsuarioUpdateRequest } from '../models/usuario';
import { paraHttpParams } from './http-params';

/** Acesso à API de usuários (/api/usuarios) — somente ADMIN. */
@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/usuarios`;

  listar(filtro: UsuarioFiltro = {}): Observable<ResultadoPaginado<Usuario>> {
    return this.http.get<ResultadoPaginado<Usuario>>(this.url, { params: paraHttpParams(filtro) });
  }

  /** Usuários ativos que podem atender chamados (TECNICO e ADMIN), em ordem alfabética. */
  listarEquipe(): Observable<Usuario[]> {
    return this.listar({ ativo: true, tamanhoPagina: 50 }).pipe(
      map((pagina) => pagina.itens.filter((usuario) => usuario.perfil !== 'USUARIO')),
    );
  }

  /** Usuários ativos (para filtros por solicitante). */
  listarAtivos(): Observable<Usuario[]> {
    return this.listar({ ativo: true, tamanhoPagina: 50 }).pipe(map((pagina) => pagina.itens));
  }

  criar(dados: UsuarioCreateRequest): Observable<Usuario> {
    return this.http.post<Usuario>(this.url, dados);
  }

  atualizar(id: number, dados: UsuarioUpdateRequest): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.url}/${id}`, dados);
  }

  desativar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  ativar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/ativar`, null);
  }
}
