import { Usuario } from './usuario';

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  tipoToken: string;
  expiraEm: string;
  usuario: Usuario;
}

export interface AlterarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
}
