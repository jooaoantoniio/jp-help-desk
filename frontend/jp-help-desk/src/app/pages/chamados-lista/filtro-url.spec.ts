import { convertToParamMap } from '@angular/router';
import { contarFiltros, filtroParaApi, filtroParaUrl, lerFiltroDaUrl } from './filtro-url';

describe('filtro-url', () => {
  it('aplica valores padrão quando a URL não tem filtros', () => {
    const filtro = lerFiltroDaUrl(convertToParamMap({}));

    expect(filtro).toMatchObject({ titulo: '', status: null, ordenarPor: 'DATA_ABERTURA', decrescente: true, pagina: 1, tamanhoPagina: 10 });
    expect(contarFiltros(filtro)).toBe(0);
  });

  it('lê filtros vindos do dashboard e descarta números inválidos', () => {
    const filtro = lerFiltroDaUrl(convertToParamMap({ status: 'EM_ATENDIMENTO', categoriaId: '3', pagina: 'abc', id: '-5' }));

    expect(filtro.status).toBe('EM_ATENDIMENTO');
    expect(filtro.categoriaId).toBe(3);
    expect(filtro.pagina).toBe(1);
    expect(filtro.id).toBeNull();
    expect(contarFiltros(filtro)).toBe(2);
  });

  it('gera URL curta, omitindo vazios e valores padrão', () => {
    const filtro = lerFiltroDaUrl(convertToParamMap({}));

    expect(filtroParaUrl({ ...filtro, prioridade: 'CRITICA', titulo: '  impressora  ' })).toEqual({
      prioridade: 'CRITICA',
      titulo: 'impressora',
    });
    expect(filtroParaUrl({ ...filtro, pagina: 3, decrescente: false, ordenarPor: 'PRIORIDADE' })).toEqual({
      pagina: 3,
      decrescente: false,
      ordenarPor: 'PRIORIDADE',
    });
  });

  it('ida e volta pela URL preserva o filtro', () => {
    const original = lerFiltroDaUrl(convertToParamMap({ status: 'ABERTO', tecnicoId: '2', de: '2026-10-01', pagina: '2' }));

    const reconstruido = lerFiltroDaUrl(convertToParamMap(filtroParaUrl(original)));

    expect(reconstruido).toEqual(original);
  });

  it('converte o período em datas ISO cobrindo o dia inteiro no fuso local', () => {
    const filtro = lerFiltroDaUrl(convertToParamMap({ de: '2026-10-01', ate: '2026-10-05' }));

    const api = filtroParaApi(filtro);

    expect(new Date(api.abertoDe!).getTime()).toBe(new Date(2026, 9, 1, 0, 0, 0, 0).getTime());
    expect(new Date(api.abertoAte!).getTime()).toBe(new Date(2026, 9, 5, 23, 59, 59, 999).getTime());
  });
});
