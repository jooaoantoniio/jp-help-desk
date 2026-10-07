import { httpResource } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Dashboard } from '../models/dashboard';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  /**
   * Recurso reativo com os indicadores do dashboard (estado de carregamento, erro e valor como signals).
   * Deve ser chamado em contexto de injeção (ex.: inicialização de um campo do componente).
   */
  carregar() {
    return httpResource<Dashboard>(() => `${environment.apiUrl}/dashboard`);
  }
}
