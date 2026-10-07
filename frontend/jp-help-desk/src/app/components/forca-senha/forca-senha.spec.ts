import { TestBed } from '@angular/core/testing';
import { ForcaSenha } from './forca-senha';

describe('ForcaSenha', () => {
  function renderizar(senha: string) {
    TestBed.configureTestingModule({ imports: [ForcaSenha] });
    const fixture = TestBed.createComponent(ForcaSenha);
    fixture.componentRef.setInput('senha', senha);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  const atendidos = (html: HTMLElement) => html.querySelectorAll('li.ok').length;

  it('sem senha: nenhum requisito atendido e sem rótulo de força', () => {
    const html = renderizar('');

    expect(html.querySelectorAll('li').length).toBe(5);
    expect(atendidos(html)).toBe(0);
    expect(html.querySelector('.nivel')?.textContent?.trim()).toBe('');
  });

  it('marca os requisitos atendidos e classifica a força', () => {
    const html = renderizar('abcdefgh1');

    expect(atendidos(html)).toBe(3);
    expect(html.querySelector('.barra')?.getAttribute('data-nivel')).toBe('media');
    expect(html.querySelector('.nivel')?.textContent).toContain('Média');
  });

  it('senha que atende tudo é "Forte"', () => {
    const html = renderizar('Senha@Forte1');

    expect(atendidos(html)).toBe(5);
    expect(html.querySelector('.nivel')?.textContent).toContain('Forte');
  });

  it('informa a leitores de tela se cada requisito foi atendido', () => {
    const html = renderizar('A');

    const textos = [...html.querySelectorAll('li .sr-only')].map((s) => s.textContent?.trim());
    expect(textos).toEqual(['(pendente)', '(atendido)', '(pendente)', '(pendente)', '(pendente)']);
  });
});
