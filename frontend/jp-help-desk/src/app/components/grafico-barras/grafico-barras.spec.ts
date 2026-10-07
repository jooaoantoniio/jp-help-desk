import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { GraficoBarras, ItemGrafico } from './grafico-barras';

describe('GraficoBarras', () => {
  function renderizar(itens: ItemGrafico[]) {
    TestBed.configureTestingModule({ imports: [GraficoBarras], providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(GraficoBarras);
    fixture.componentRef.setInput('itens', itens);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  it('dimensiona as barras em relação ao maior valor e mostra o percentual do total', () => {
    const html = renderizar([
      { rotulo: 'Aberto', valor: 6, cor: 'blue' },
      { rotulo: 'Fechado', valor: 3, cor: 'gray' },
      { rotulo: 'Cancelado', valor: 0, cor: 'red' },
    ]);

    const barras = [...html.querySelectorAll<HTMLElement>('.grafico__barra')];
    expect(barras.map((b) => b.style.width)).toEqual(['100%', '50%', '0%']);

    const valores = [...html.querySelectorAll('.grafico__valor')].map((v) => v.textContent?.replace(/\s+/g, ' ').trim());
    expect(valores).toEqual(['6 67%', '3 33%', '0 0%']);
  });

  it('descreve cada barra para leitores de tela', () => {
    const html = renderizar([{ rotulo: 'Crítica', valor: 2, cor: 'red' }]);

    expect(html.querySelector('a')?.getAttribute('aria-label')).toBe('Crítica: 2 chamados (100%)');
  });

  it('mostra mensagem quando não há dados', () => {
    const html = renderizar([]);

    expect(html.textContent).toContain('Sem dados para exibir.');
  });
});
