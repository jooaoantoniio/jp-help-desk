import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { salvarSessao } from '../testing/sessao';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  function configurar() {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    const auth = TestBed.inject(AuthService);
    return {
      http: TestBed.inject(HttpClient),
      controller: TestBed.inject(HttpTestingController),
      auth,
      logout: vi.spyOn(auth, 'logout').mockImplementation(() => undefined),
    };
  }

  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('envia o token para a nossa API', () => {
    salvarSessao(30);
    const { http, controller } = configurar();

    http.get('/api/chamados').subscribe();

    expect(controller.expectOne('/api/chamados').request.headers.get('Authorization')).toBe('Bearer token-salvo');
  });

  it('não envia o token para outros domínios', () => {
    salvarSessao(30);
    const { http, controller } = configurar();

    http.get('https://outro-site.com/dados').subscribe();

    expect(controller.expectOne('https://outro-site.com/dados').request.headers.has('Authorization')).toBe(false);
  });

  it('não envia cabeçalho quando não há sessão', () => {
    const { http, controller } = configurar();

    http.get('/api/health').subscribe();

    expect(controller.expectOne('/api/health').request.headers.has('Authorization')).toBe(false);
  });

  it('faz logout por sessão expirada ao receber 401 com token', () => {
    salvarSessao(30);
    const { http, controller, logout } = configurar();

    http.get('/api/chamados').subscribe({ error: () => undefined });
    controller.expectOne('/api/chamados').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).toHaveBeenCalledWith('expirada');
  });

  it('não faz logout em 401 sem token (ex.: senha errada no login)', () => {
    const { http, controller, logout } = configurar();

    http.post('/api/auth/login', {}).subscribe({ error: () => undefined });
    controller.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).not.toHaveBeenCalled();
  });

  it('não faz logout em 403 (sem permissão não significa sessão expirada)', () => {
    salvarSessao(30);
    const { http, controller, logout } = configurar();

    http.get('/api/usuarios').subscribe({ error: () => undefined });
    controller.expectOne('/api/usuarios').flush(null, { status: 403, statusText: 'Forbidden' });

    expect(logout).not.toHaveBeenCalled();
  });
});
