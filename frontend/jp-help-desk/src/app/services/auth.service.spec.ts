import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { LoginResponse } from '../models/auth';
import { Usuario } from '../models/usuario';
import { salvarSessao, usuarioTeste } from '../testing/sessao';
import { AuthService, CHAVE_SESSAO } from './auth.service';

describe('AuthService', () => {
  function criarServico() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    return {
      auth: TestBed.inject(AuthService),
      http: TestBed.inject(HttpTestingController),
      router: TestBed.inject(Router),
    };
  }

  beforeEach(() => localStorage.clear());
  afterEach(() => {
    TestBed.inject(HttpTestingController).verify();
    localStorage.clear();
  });

  it('começa deslogado quando não há sessão salva', () => {
    const { auth } = criarServico();

    expect(auth.estaLogado()).toBe(false);
    expect(auth.usuario()).toBeNull();
    expect(auth.token()).toBeNull();
  });

  it('login guarda a sessão e atualiza os signals', () => {
    const { auth, http } = criarServico();
    let usuarioRetornado: Usuario | undefined;

    auth.login({ email: 'usuario@jphelpdesk.com', senha: 'Senha@123' }).subscribe((u) => (usuarioRetornado = u));

    const req = http.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'usuario@jphelpdesk.com', senha: 'Senha@123' });

    const resposta: LoginResponse = {
      token: 'jwt-novo',
      tipoToken: 'Bearer',
      expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
      usuario: usuarioTeste,
    };
    req.flush(resposta);

    expect(usuarioRetornado).toEqual(usuarioTeste);
    expect(auth.estaLogado()).toBe(true);
    expect(auth.token()).toBe('jwt-novo');
    expect(auth.perfil()).toBe('USUARIO');
    expect(JSON.parse(localStorage.getItem(CHAVE_SESSAO)!).token).toBe('jwt-novo');
  });

  it('login com credenciais inválidas não cria sessão', () => {
    const { auth, http } = criarServico();
    let erroRecebido = false;

    auth.login({ email: 'x@x.com', senha: 'errada' }).subscribe({ error: () => (erroRecebido = true) });
    http.expectOne('/api/auth/login').flush({ detail: 'E-mail ou senha inválidos.' }, { status: 401, statusText: 'Unauthorized' });

    expect(erroRecebido).toBe(true);
    expect(auth.estaLogado()).toBe(false);
    expect(localStorage.getItem(CHAVE_SESSAO)).toBeNull();
  });

  it('restaura uma sessão válida salva no localStorage', () => {
    salvarSessao(30);
    const { auth } = criarServico();

    expect(auth.estaLogado()).toBe(true);
    expect(auth.usuario()?.nome).toBe('Usuário Padrão');
  });

  it('descarta uma sessão expirada salva no localStorage', () => {
    salvarSessao(-1);
    const { auth } = criarServico();

    expect(auth.estaLogado()).toBe(false);
    expect(localStorage.getItem(CHAVE_SESSAO)).toBeNull();
  });

  it('descarta uma sessão corrompida', () => {
    localStorage.setItem(CHAVE_SESSAO, '{json-invalido');
    const { auth } = criarServico();

    expect(auth.estaLogado()).toBe(false);
    expect(localStorage.getItem(CHAVE_SESSAO)).toBeNull();
  });

  it('logout limpa a sessão e volta ao login', () => {
    salvarSessao(30);
    const { auth, router } = criarServico();
    const navegar = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    auth.logout();

    expect(auth.estaLogado()).toBe(false);
    expect(localStorage.getItem(CHAVE_SESSAO)).toBeNull();
    expect(navegar).toHaveBeenCalledWith(['/login'], { queryParams: {} });
  });

  it('encerra a sessão automaticamente quando o token expira', () => {
    vi.useFakeTimers();
    try {
      salvarSessao(1);
      const { auth, router } = criarServico();
      const navegar = vi.spyOn(router, 'navigate').mockResolvedValue(true);

      vi.advanceTimersByTime(59_000);
      expect(auth.estaLogado()).toBe(true);

      vi.advanceTimersByTime(2_000);
      expect(auth.estaLogado()).toBe(false);
      expect(navegar).toHaveBeenCalledWith(['/login'], { queryParams: { motivo: 'expirada' } });
    } finally {
      vi.useRealTimers();
    }
  });

  it('temPerfil verifica o perfil do usuário logado', () => {
    salvarSessao(30, { ...usuarioTeste, perfil: 'TECNICO' });
    const { auth } = criarServico();

    expect(auth.temPerfil('ADMIN', 'TECNICO')).toBe(true);
    expect(auth.temPerfil('ADMIN')).toBe(false);
  });
});
