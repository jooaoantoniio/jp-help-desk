import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Usuario } from '../../models/usuario';
import { UsuarioDialog, UsuarioDialogDados } from './usuario-dialog';

describe('UsuarioDialog', () => {
  const usuarioExistente: Usuario = {
    id: 5,
    nome: 'Maria Oliveira',
    email: 'maria@jphelpdesk.com',
    perfil: 'TECNICO',
    ativo: true,
    criadoEm: '2026-10-07T00:00:00Z',
    atualizadoEm: null,
  };

  function abrir(dados: UsuarioDialogDados) {
    const dialogRef = { close: vi.fn(), disableClose: false };
    TestBed.configureTestingModule({
      imports: [UsuarioDialog],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: MAT_DIALOG_DATA, useValue: dados },
        { provide: MatDialogRef, useValue: dialogRef },
      ],
    });
    const fixture = TestBed.createComponent(UsuarioDialog);
    fixture.detectChanges();
    const html = fixture.nativeElement as HTMLElement;

    const digitar = (controle: string, valor: string) => {
      const input = html.querySelector<HTMLInputElement>(`input[formcontrolname=${controle}]`)!;
      input.value = valor;
      input.dispatchEvent(new Event('input'));
    };
    const enviar = () => {
      html.querySelector('form')!.dispatchEvent(new Event('submit'));
      fixture.detectChanges();
    };

    return { fixture, html, dialogRef, digitar, enviar, http: TestBed.inject(HttpTestingController) };
  }

  it('criação: envia os dados com o perfil escolhido e fecha com o usuário salvo', () => {
    const { dialogRef, digitar, enviar, http } = abrir({ usuario: null, ehVoceMesmo: false });
    digitar('nome', '  Ana Souza ');
    digitar('email', 'ana@jphelpdesk.com');
    digitar('senha', 'Senha@Forte1');

    enviar();

    const requisicao = http.expectOne({ method: 'POST', url: '/api/usuarios' });
    expect(requisicao.request.body).toEqual({ nome: 'Ana Souza', email: 'ana@jphelpdesk.com', perfil: 'USUARIO', senha: 'Senha@Forte1' });
    const salvo = { ...usuarioExistente, id: 9, nome: 'Ana Souza' };
    requisicao.flush(salvo);
    expect(dialogRef.close).toHaveBeenCalledWith(salvo);
  });

  it('não envia com senha fraca', () => {
    const { html, digitar, enviar, http } = abrir({ usuario: null, ehVoceMesmo: false });
    digitar('nome', 'Ana Souza');
    digitar('email', 'ana@jphelpdesk.com');
    digitar('senha', 'fraca');

    enviar();

    http.expectNone('/api/usuarios');
    expect(html.textContent).toContain('A senha não atende a todos os requisitos.');
  });

  it('edição: não tem campo de senha e envia PUT sem senha', () => {
    const { html, enviar, http } = abrir({ usuario: usuarioExistente, ehVoceMesmo: false });

    expect(html.querySelector('input[formcontrolname=senha]')).toBeNull();
    enviar();

    const requisicao = http.expectOne({ method: 'PUT', url: '/api/usuarios/5' });
    expect(requisicao.request.body).toEqual({ nome: 'Maria Oliveira', email: 'maria@jphelpdesk.com', perfil: 'TECNICO' });
  });

  it('editando a própria conta: perfil bloqueado, mas enviado à API', () => {
    const { html, enviar, http } = abrir({ usuario: { ...usuarioExistente, perfil: 'ADMIN' }, ehVoceMesmo: true });

    expect(html.querySelector('mat-select')?.getAttribute('aria-disabled')).toBe('true');
    expect(html.textContent).toContain('Você não pode alterar o seu próprio perfil.');
    enviar();

    expect(http.expectOne('/api/usuarios/5').request.body.perfil).toBe('ADMIN');
  });

  it('e-mail em uso (409): mostra o erro no campo e mantém o diálogo aberto', () => {
    const { fixture, html, dialogRef, enviar, http } = abrir({ usuario: usuarioExistente, ehVoceMesmo: false });
    enviar();

    http.expectOne('/api/usuarios/5').flush({ detail: 'Em uso' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect(html.textContent).toContain('Este e-mail já está em uso por outro usuário.');
    expect(dialogRef.close).not.toHaveBeenCalled();
  });
});
