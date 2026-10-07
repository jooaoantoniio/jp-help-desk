import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/comum';

const MENSAGEM_PADRAO = 'Ocorreu um erro inesperado. Tente novamente.';

/**
 * Converte um erro HTTP em uma mensagem amigável para exibir ao usuário,
 * aproveitando o ProblemDetails retornado pela API.
 */
export function mensagemErro(erro: unknown, padrao = MENSAGEM_PADRAO): string {
  if (!(erro instanceof HttpErrorResponse)) {
    return padrao;
  }

  // 0 = sem resposta (rede/CORS); 502/503/504 = servidor ou proxy indisponível.
  if (erro.status === 0 || [502, 503, 504].includes(erro.status)) {
    return 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';
  }

  if (erro.status === 429) {
    return 'Muitas tentativas em pouco tempo. Aguarde um minuto e tente novamente.';
  }

  const problema = erro.error as ProblemDetails | null;

  // Erro de validação (400): mostra a primeira mensagem dos campos.
  const primeiraValidacao = problema?.errors ? Object.values(problema.errors).flat()[0] : undefined;

  return primeiraValidacao ?? problema?.detail ?? padrao;
}
