import { Usuario } from '../models/usuario';
import { CHAVE_SESSAO } from '../services/auth.service';

// Utilitários compartilhados pelos testes unitários (não fazem parte do build da aplicação).

export const usuarioTeste: Usuario = {
  id: 3,
  nome: 'Usuário Padrão',
  email: 'usuario@jphelpdesk.com',
  perfil: 'USUARIO',
  ativo: true,
  criadoEm: '2026-10-07T00:00:00Z',
  atualizadoEm: null,
};

/** Grava uma sessão no localStorage, como se o usuário já tivesse feito login antes. */
export function salvarSessao(minutosParaExpirar: number, usuario: Usuario = usuarioTeste): void {
  const expiraEm = new Date(Date.now() + minutosParaExpirar * 60_000).toISOString();
  localStorage.setItem(CHAVE_SESSAO, JSON.stringify({ token: 'token-salvo', expiraEm, usuario }));
}
