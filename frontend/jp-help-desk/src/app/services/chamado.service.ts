import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AlterarStatusRequest, Chamado, ChamadoFiltro, ChamadoRequest, Historico } from '../models/chamado';
import { ResultadoPaginado } from '../models/comum';
import { paraHttpParams } from './http-params';

/** Acesso à API de chamados (/api/chamados). */
@Injectable({ providedIn: 'root' })
export class ChamadoService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/chamados`;

  listar(filtro: ChamadoFiltro): Observable<ResultadoPaginado<Chamado>> {
    return this.http.get<ResultadoPaginado<Chamado>>(this.url, { params: paraHttpParams(filtro) });
  }

  obter(id: number): Observable<Chamado> {
    return this.http.get<Chamado>(`${this.url}/${id}`);
  }

  abrir(dados: ChamadoRequest): Observable<Chamado> {
    return this.http.post<Chamado>(this.url, dados);
  }

  atualizar(id: number, dados: ChamadoRequest): Observable<Chamado> {
    return this.http.put<Chamado>(`${this.url}/${id}`, dados);
  }

  alterarStatus(id: number, dados: AlterarStatusRequest): Observable<Chamado> {
    return this.http.patch<Chamado>(`${this.url}/${id}/status`, dados);
  }

  atribuir(id: number, tecnicoId: number): Observable<Chamado> {
    return this.http.patch<Chamado>(`${this.url}/${id}/atribuir`, { tecnicoId });
  }

  assumir(id: number): Observable<Chamado> {
    return this.http.patch<Chamado>(`${this.url}/${id}/assumir`, null);
  }

  cancelar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  historico(id: number): Observable<Historico[]> {
    return this.http.get<Historico[]>(`${this.url}/${id}/historico`);
  }

  comentar(id: number, texto: string): Observable<Historico> {
    return this.http.post<Historico>(`${this.url}/${id}/comentarios`, { texto });
  }
}
