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

/** Iniciais para avatar (ex.: "Técnico de Suporte" -> "TS"). */
export function iniciais(nome: string): string {
  const partes = nome.trim().split(/\s+/).filter(Boolean);
  const primeira = partes.at(0)?.[0] ?? '';
  const ultima = partes.length > 1 ? (partes.at(-1)?.[0] ?? '') : '';
  return (primeira + ultima).toUpperCase();
}
