import { FormControl, FormGroup } from '@angular/forms';
import { confirmaCampo, nivelSenha, senhaForte } from './senha';

describe('regras de senha', () => {
  it('classifica a força da senha', () => {
    expect(nivelSenha('')).toBe('vazia');
    expect(nivelSenha('abc')).toBe('fraca');
    expect(nivelSenha('abcdefgh')).toBe('fraca'); // 2 de 5
    expect(nivelSenha('abcdefgh1')).toBe('media'); // 3 de 5
    expect(nivelSenha('Abcdefgh1')).toBe('media');
    expect(nivelSenha('Abcdefg1!')).toBe('forte');
  });

  it('senhaForte lista os requisitos pendentes', () => {
    expect(senhaForte(new FormControl(''))).toBeNull();
    expect(senhaForte(new FormControl('Senha@Forte1'))).toBeNull();

    const erro = senhaForte(new FormControl('senha'));
    expect(erro?.['senhaFraca'].pendentes).toEqual([
      'Pelo menos 8 caracteres',
      'Uma letra maiúscula',
      'Um número',
      'Um símbolo (ex.: @ # ! ?)',
    ]);
  });

  it('aceita como especial os mesmos caracteres que a API (inclusive acentos)', () => {
    expect(senhaForte(new FormControl('Senhaforte1é'))).toBeNull();
  });

  it('confirmaCampo compara com o campo irmão', () => {
    const form = new FormGroup({
      novaSenha: new FormControl('Senha@Forte1'),
      confirmacao: new FormControl('', confirmaCampo('novaSenha')),
    });
    const confirmacao = form.controls.confirmacao;

    expect(confirmacao.errors).toBeNull(); // vazio: quem reclama é o "required"
    confirmacao.setValue('Outra@Senha1');
    expect(confirmacao.hasError('senhasDiferentes')).toBe(true);
    confirmacao.setValue('Senha@Forte1');
    expect(confirmacao.errors).toBeNull();
  });
});
