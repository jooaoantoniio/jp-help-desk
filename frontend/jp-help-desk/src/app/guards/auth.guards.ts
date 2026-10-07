import { inject, Injector } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PerfilUsuario } from '../models/enums';
import { AuthService } from '../services/auth.service';

/** Exige usuário logado; senão, vai ao login guardando o destino em "returnUrl". */
export const authGuard: CanActivateFn = (_route, state) => {
  if (inject(AuthService).estaLogado()) {
    return true;
  }

  return inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/** Para a tela de login: quem já está logado vai direto ao dashboard. */
export const visitanteGuard: CanActivateFn = () =>
  inject(AuthService).estaLogado() ? inject(Router).createUrlTree(['/dashboard']) : true;

/**
 * Restringe a rota a determinados perfis. Ex.: canActivate: [perfilGuard('ADMIN')].
 * A API também valida (403) — o guard apenas evita mostrar telas inacessíveis.
 */
export function perfilGuard(...perfis: PerfilUsuario[]): CanActivateFn {
  return () => {
    if (inject(AuthService).temPerfil(...perfis)) {
      return true;
    }

    avisarAcessoNegado(inject(Injector));
    return inject(Router).createUrlTree(['/dashboard']);
  };
}

/**
 * O snackbar é carregado sob demanda (import dinâmico): assim o Material Snack Bar
 * não entra no pacote inicial que todo usuário baixa ao abrir o sistema.
 */
function avisarAcessoNegado(injector: Injector): void {
  void import('@angular/material/snack-bar').then(({ MatSnackBar }) =>
    injector.get(MatSnackBar).open('Você não tem permissão para acessar esta página.', 'OK', { duration: 4000 }),
  );
}
