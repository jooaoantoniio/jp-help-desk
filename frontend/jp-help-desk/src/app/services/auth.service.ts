import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { map, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AlterarSenhaRequest, LoginRequest, LoginResponse } from '../models/auth';
import { PerfilUsuario } from '../models/enums';
import { Usuario } from '../models/usuario';

/** Chave do localStorage onde a sessão é mantida entre recarregamentos da página. */
export const CHAVE_SESSAO = 'jp-helpdesk.sessao';

interface Sessao {
  token: string;
  expiraEm: string;
  usuario: Usuario;
}

/**
 * Estado de autenticação da aplicação (fonte única de verdade).
 * Expõe signals somente leitura; a sessão é persistida no localStorage e
 * encerrada automaticamente quando o token expira.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly url = `${environment.apiUrl}/auth`;

  private readonly sessao = signal<Sessao | null>(lerSessaoSalva());
  private timerExpiracao?: ReturnType<typeof setTimeout>;

  readonly usuario = computed(() => this.sessao()?.usuario ?? null);
  readonly estaLogado = computed(() => this.sessao() !== null);
  readonly perfil = computed(() => this.usuario()?.perfil ?? null);

  constructor() {
    this.agendarExpiracao();
  }

  /** Token JWT atual (usado pelo interceptor). */
  token(): string | null {
    return this.sessao()?.token ?? null;
  }

  temPerfil(...perfis: PerfilUsuario[]): boolean {
    const perfil = this.perfil();
    return perfil !== null && perfis.includes(perfil);
  }

  login(credenciais: LoginRequest): Observable<Usuario> {
    return this.http.post<LoginResponse>(`${this.url}/login`, credenciais).pipe(
      tap((resposta) =>
        this.definirSessao({ token: resposta.token, expiraEm: resposta.expiraEm, usuario: resposta.usuario }),
      ),
      map((resposta) => resposta.usuario),
    );
  }

  /** Encerra a sessão e volta ao login. "expirada" exibe o aviso de sessão expirada. */
  logout(motivo?: 'expirada'): void {
    this.definirSessao(null);
    void this.router.navigate(['/login'], { queryParams: motivo ? { motivo } : {} });
  }

  /** Recarrega os dados do usuário logado a partir da API (ex.: após editar o perfil). */
  recarregarUsuario(): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.url}/me`).pipe(tap((usuario) => this.atualizarUsuario(usuario)));
  }

  alterarSenha(dados: AlterarSenhaRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/senha`, dados);
  }

  private atualizarUsuario(usuario: Usuario): void {
    const atual = this.sessao();
    if (atual) {
      this.definirSessao({ ...atual, usuario });
    }
  }

  private definirSessao(sessao: Sessao | null): void {
    this.sessao.set(sessao);

    if (sessao) {
      localStorage.setItem(CHAVE_SESSAO, JSON.stringify(sessao));
    } else {
      localStorage.removeItem(CHAVE_SESSAO);
    }

    this.agendarExpiracao();
  }

  /** Programa o logout automático para o instante em que o token expira. */
  private agendarExpiracao(): void {
    clearTimeout(this.timerExpiracao);

    const sessao = this.sessao();
    if (!sessao) {
      return;
    }

    // setTimeout aceita no máximo ~24,8 dias; tokens duram bem menos, mas limitamos por segurança.
    const restanteMs = Math.min(new Date(sessao.expiraEm).getTime() - Date.now(), 2_147_483_647);
    this.timerExpiracao = setTimeout(() => this.logout('expirada'), Math.max(restanteMs, 0));
  }
}

/** Lê a sessão salva, descartando-a se estiver corrompida ou expirada. */
function lerSessaoSalva(): Sessao | null {
  try {
    const json = localStorage.getItem(CHAVE_SESSAO);
    if (!json) {
      return null;
    }

    const sessao = JSON.parse(json) as Sessao;
    if (!sessao.token || new Date(sessao.expiraEm).getTime() <= Date.now()) {
      localStorage.removeItem(CHAVE_SESSAO);
      return null;
    }

    return sessao;
  } catch {
    localStorage.removeItem(CHAVE_SESSAO);
    return null;
  }
}
