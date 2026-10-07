import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Espelho da política de senha da API (RegrasSenha.cs): mínimo de 8 caracteres, com letra
// maiúscula, minúscula, número e caractere especial. A API continua validando — aqui é para
// orientar quem digita antes de enviar.

export const LIMITES_SENHA = { min: 8, max: 100 } as const;

export interface RequisitoSenha {
  rotulo: string;
  atende: (senha: string) => boolean;
}

export const REQUISITOS_SENHA: readonly RequisitoSenha[] = [
  { rotulo: `Pelo menos ${LIMITES_SENHA.min} caracteres`, atende: (s) => s.length >= LIMITES_SENHA.min },
  { rotulo: 'Uma letra maiúscula', atende: (s) => /[A-Z]/.test(s) },
  { rotulo: 'Uma letra minúscula', atende: (s) => /[a-z]/.test(s) },
  { rotulo: 'Um número', atende: (s) => /\d/.test(s) },
  // Mesmo critério da API ([^a-zA-Z\d]): qualquer coisa que não seja letra sem acento ou dígito.
  { rotulo: 'Um símbolo (ex.: @ # ! ?)', atende: (s) => /[^a-zA-Z\d]/.test(s) },
];

export type NivelSenha = 'vazia' | 'fraca' | 'media' | 'forte';

/** Força da senha pela quantidade de requisitos atendidos (forte = todos). */
export function nivelSenha(senha: string): NivelSenha {
  if (!senha) {
    return 'vazia';
  }
  const atendidos = REQUISITOS_SENHA.filter((requisito) => requisito.atende(senha)).length;
  if (atendidos === REQUISITOS_SENHA.length) {
    return 'forte';
  }
  return atendidos >= 3 ? 'media' : 'fraca';
}

/** Erro "senhaFraca" enquanto algum requisito não for atendido (campo vazio fica com o "required"). */
export const senhaForte: ValidatorFn = (controle: AbstractControl): ValidationErrors | null => {
  const senha = (controle.value as string | null) ?? '';
  if (!senha) {
    return null;
  }
  const pendentes = REQUISITOS_SENHA.filter((requisito) => !requisito.atende(senha)).map((r) => r.rotulo);
  return pendentes.length ? { senhaFraca: { pendentes } } : null;
};

/**
 * Erro "senhasDiferentes" quando o valor difere do campo irmão (ex.: confirmação da nova senha).
 * O componente deve revalidar a confirmação quando o campo original mudar.
 */
export function confirmaCampo(nomeCampo: string): ValidatorFn {
  return (controle: AbstractControl): ValidationErrors | null => {
    const original = controle.parent?.get(nomeCampo)?.value as string | undefined;
    return controle.value && original !== controle.value ? { senhasDiferentes: true } : null;
  };
}
