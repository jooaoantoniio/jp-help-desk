import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { salvarSessao, usuarioTeste } from '../testing/sessao';
import { authGuard, perfilGuard, visitanteGuard } from './auth.guards';

describe('auth guards', () => {
  const rota = {} as ActivatedRouteSnapshot;
  const estado = (url: string) => ({ url }) as RouterStateSnapshot;

  function configurar() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    return TestBed.inject(Router);
  }

  /** Executa o guard dentro do contexto de injeção do Angular (necessário para inject()). */
  const executar = (guard: typeof authGuard, url = '/') =>
    TestBed.runInInjectionContext(() => guard(rota, estado(url)));

  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('authGuard libera usuário logado', () => {
    salvarSessao(30);
    configurar();

    expect(executar(authGuard)).toBe(true);
  });

  it('authGuard manda o visitante ao login guardando o destino', () => {
    const router = configurar();

    const resultado = executar(authGuard, '/chamados/15') as UrlTree;

    expect(router.serializeUrl(resultado)).toBe('/login?returnUrl=%2Fchamados%2F15');
  });

  it('visitanteGuard leva quem já está logado ao dashboard', () => {
    salvarSessao(30);
    const router = configurar();

    expect(router.serializeUrl(executar(visitanteGuard) as UrlTree)).toBe('/dashboard');
  });

  it('perfilGuard libera o perfil permitido', () => {
    salvarSessao(30, { ...usuarioTeste, perfil: 'ADMIN' });
    configurar();

    expect(executar(perfilGuard('ADMIN'))).toBe(true);
  });

  it('perfilGuard bloqueia outros perfis, volta ao dashboard e avisa o usuário', async () => {
    salvarSessao(30, { ...usuarioTeste, perfil: 'USUARIO' });
    const router = configurar();

    expect(router.serializeUrl(executar(perfilGuard('ADMIN')) as UrlTree)).toBe('/dashboard');

    // O snackbar é carregado sob demanda: aguarda ele aparecer antes de o teste terminar.
    await vi.waitFor(() =>
      expect(document.body.textContent).toContain('Você não tem permissão para acessar esta página.'),
    );
  });
});
