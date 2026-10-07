import { HttpErrorResponse } from '@angular/common/http';
import { mensagemErro } from './erro-api';

describe('mensagemErro', () => {
  const erroHttp = (status: number, error: unknown = null) => new HttpErrorResponse({ status, error });
  const SEM_CONEXAO = 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';

  it('usa o "detail" do ProblemDetails retornado pela API', () => {
    expect(mensagemErro(erroHttp(404, { detail: 'Chamado 99 não encontrado.' }))).toBe('Chamado 99 não encontrado.');
  });

  it('usa a primeira mensagem de validação em erros 400', () => {
    const erro = erroHttp(400, { title: 'One or more validation errors occurred.', errors: { Titulo: ['O título é obrigatório.'] } });
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
