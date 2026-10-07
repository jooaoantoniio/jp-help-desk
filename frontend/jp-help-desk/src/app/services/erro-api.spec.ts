import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { aplicarErrosDaApi, mensagemErro } from './erro-api';

describe('mensagemErro', () => {
  const erroHttp = (status: number, error: unknown = null) => new HttpErrorResponse({ status, error });
  const SEM_CONEXAO = 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';

  it('usa o "detail" do ProblemDetails retornado pela API', () => {
    expect(mensagemErro(erroHttp(404, { detail: 'Chamado 99 não encontrado.' }))).toBe('Chamado 99 não encontrado.');
  });

  it('usa a primeira mensagem de validação em erros 400', () => {
    const erro = erroHttp(400, { title: 'Um ou mais campos são inválidos.', errors: { titulo: ['O título é obrigatório.'] } });
    expect(mensagemErro(erro)).toBe('O título é obrigatório.');
  });

  it.each([0, 502, 503, 504])('trata o status %i como servidor indisponível', (status) => {
    expect(mensagemErro(erroHttp(status))).toBe(SEM_CONEXAO);
  });

  it('explica o limite de tentativas (429)', () => {
    expect(mensagemErro(erroHttp(429))).toContain('Muitas tentativas');
  });

  it('usa a mensagem padrão quando não há detalhes', () => {
    expect(mensagemErro(erroHttp(500))).toBe('Ocorreu um erro inesperado. Tente novamente.');
    expect(mensagemErro(new Error('qualquer'), 'Padrão da tela')).toBe('Padrão da tela');
  });
});

describe('aplicarErrosDaApi', () => {
  const novoForm = () =>
    new FormGroup({
      nome: new FormControl('Ab', Validators.required),
      email: new FormControl('a@b.com'),
    });

  it('marca o campo com a mensagem da API (chave camelCase = formControlName)', () => {
    const form = novoForm();
    const erro = new HttpErrorResponse({ status: 400, error: { errors: { nome: ['O nome deve ter entre 3 e 100 caracteres.'], corpo: ['x'] } } });

    expect(aplicarErrosDaApi(form, erro)).toBe(true);
    expect(form.controls.nome.getError('api')).toBe('O nome deve ter entre 3 e 100 caracteres.');
    expect(form.controls.nome.touched).toBe(true);
    expect(form.controls.email.valid).toBe(true);
  });

  it('o erro some quando o campo é editado', () => {
    const form = novoForm();
    aplicarErrosDaApi(form, new HttpErrorResponse({ status: 400, error: { errors: { nome: ['Inválido.'] } } }));
    form.controls.nome.setValue('Abc');
    expect(form.controls.nome.hasError('api')).toBe(false);
  });

  it('ignora erros que não são de validação ou sem campo correspondente', () => {
    const form = novoForm();
    expect(aplicarErrosDaApi(form, new HttpErrorResponse({ status: 409, error: { detail: 'Conflito' } }))).toBe(false);
    expect(aplicarErrosDaApi(form, new HttpErrorResponse({ status: 400, error: { errors: { outro: ['x'] } } }))).toBe(false);
    expect(form.valid).toBe(true);
  });
});
