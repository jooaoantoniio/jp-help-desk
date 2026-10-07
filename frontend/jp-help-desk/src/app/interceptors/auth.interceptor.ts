import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

/**
 * Anexa o token JWT às requisições para a nossa API e trata sessão expirada:
 * um 401 em requisição que levava token significa que ele não vale mais → logout.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();

  // O token só é enviado para a nossa API — nunca para outros domínios.
  const ehNossaApi = req.url.startsWith(environment.apiUrl);
  const requisicao = token && ehNossaApi ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(requisicao).pipe(
    catchError((erro: unknown) => {
      if (erro instanceof HttpErrorResponse && erro.status === 401 && ehNossaApi && token) {
        auth.logout('expirada');
      }
      return throwError(() => erro);
    }),
  );
};
