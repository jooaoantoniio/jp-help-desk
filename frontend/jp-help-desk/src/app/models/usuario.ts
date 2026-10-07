import { Paginacao } from './comum';
import { PerfilUsuario } from './enums';

export interface Usuario {
  id: number;
  nome: string;
  email: string;
  perfil: PerfilUsuario;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string | null;
}

export interface UsuarioCreateRequest {
  nome: string;
  email: string;
  senha: string;
  perfil: PerfilUsuario;
}

export interface UsuarioUpdateRequest {
  nome: string;
  email: string;
  perfil: PerfilUsuario;
}

export interface UsuarioFiltro extends Paginacao {
  busca?: string;
  perfil?: PerfilUsuario;
  ativo?: boolean;
}
