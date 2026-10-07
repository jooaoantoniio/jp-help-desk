import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { Observable, finalize } from 'rxjs';
import { ForcaSenha } from '../../components/forca-senha/forca-senha';
import { PERFIL_LABEL, PerfilUsuario } from '../../models/enums';
import { Usuario } from '../../models/usuario';
import { aplicarErrosDaApi, mensagemErro } from '../../services/erro-api';
import { UsuarioService } from '../../services/usuario.service';
import { LIMITES_SENHA, senhaForte } from '../../validacao/senha';

/** Mesmos limites do UsuarioCreateRequest / UsuarioUpdateRequest da API. */
const LIMITES = { nomeMin: 3, nomeMax: 100, emailMax: 150 } as const;

export interface UsuarioDialogDados {
  /** null = novo usuário. */
  usuario: Usuario | null;
  /** O ADMIN está editando a própria conta. */
  ehVoceMesmo: boolean;
}

/**
 * Cria ou edita um usuário (o próprio diálogo chama a API).
 * Fecha com o usuário salvo, ou undefined ao desistir.
 */
@Component({
  selector: 'app-usuario-dialog',
  imports: [
    ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatIconModule, MatProgressBarModule, ForcaSenha,
  ],
  template: `
    <h2 mat-dialog-title>{{ editando ? 'Editar usuário' : 'Novo usuário' }}</h2>
    <form [formGroup]="form" (ngSubmit)="salvar()" novalidate>
      <mat-dialog-content>
        @if (salvando()) {
          <mat-progress-bar mode="indeterminate" class="progresso" />
        }
        @if (erro(); as mensagem) {
          <div class="aviso aviso--erro" role="alert">
            <mat-icon aria-hidden="true">error</mat-icon>
            <span>{{ mensagem }}</span>
          </div>
        }

        <mat-form-field appearance="outline">
          <mat-label>Nome completo</mat-label>
          <input matInput formControlName="nome" [maxlength]="limites.nomeMax" autocomplete="off" cdkFocusInitial />
          @if (form.controls.nome.hasError('required')) {
            <mat-error>Informe o nome.</mat-error>
          } @else if (form.controls.nome.hasError('minlength')) {
            <mat-error>O nome deve ter pelo menos {{ limites.nomeMin }} caracteres.</mat-error>
          } @else if (form.controls.nome.hasError('api')) {
            <mat-error>{{ form.controls.nome.getError('api') }}</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>E-mail</mat-label>
          <input matInput type="email" formControlName="email" [maxlength]="limites.emailMax" autocomplete="off" />
          @if (form.controls.email.hasError('required')) {
            <mat-error>Informe o e-mail.</mat-error>
          } @else if (form.controls.email.hasError('email')) {
            <mat-error>Informe um e-mail válido.</mat-error>
          } @else if (form.controls.email.hasError('emUso')) {
            <mat-error>Este e-mail já está em uso por outro usuário.</mat-error>
          } @else if (form.controls.email.hasError('api')) {
            <mat-error>{{ form.controls.email.getError('api') }}</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Perfil de acesso</mat-label>
          <mat-select formControlName="perfil">
            @for (opcao of perfis; track opcao[0]) {
              <mat-option [value]="opcao[0]">{{ opcao[1] }}</mat-option>
            }
          </mat-select>
          @if (dados.ehVoceMesmo) {
            <mat-hint>Você não pode alterar o seu próprio perfil.</mat-hint>
          } @else {
            <mat-hint>{{ descricaoPerfil[form.controls.perfil.value] }}</mat-hint>
          }
        </mat-form-field>

        @if (!editando) {
          <mat-form-field appearance="outline">
            <mat-label>Senha inicial</mat-label>
            <input matInput [type]="mostrarSenha() ? 'text' : 'password'" formControlName="senha"
              [maxlength]="limiteSenha" autocomplete="new-password" />
            <button mat-icon-button matSuffix type="button" (click)="mostrarSenha.update((v) => !v)"
              [attr.aria-label]="mostrarSenha() ? 'Ocultar senha' : 'Mostrar senha'" [attr.aria-pressed]="mostrarSenha()">
              <mat-icon>{{ mostrarSenha() ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (form.controls.senha.hasError('required')) {
              <mat-error>Informe a senha inicial.</mat-error>
            } @else if (form.controls.senha.hasError('senhaFraca')) {
              <mat-error>A senha não atende a todos os requisitos.</mat-error>
            } @else if (form.controls.senha.hasError('api')) {
              <mat-error>{{ form.controls.senha.getError('api') }}</mat-error>
            }
          </mat-form-field>
          <app-forca-senha [senha]="form.controls.senha.value" />
          <p class="dica">Informe a senha à pessoa por um canal seguro. Ela poderá trocá-la em "Meu perfil".</p>
        }
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close [disabled]="salvando()">Cancelar</button>
        <button mat-flat-button type="submit" [disabled]="salvando()">{{ editando ? 'Salvar' : 'Criar usuário' }}</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    mat-dialog-content { display: flex; flex-direction: column; gap: 8px; padding-top: 8px !important; }
    .progresso { position: absolute; inset: 0 0 auto; }
    .dica { margin: 0; color: var(--jp-text-muted); font-size: 13px; }
  `,
})
export class UsuarioDialog {
  protected readonly dados = inject<UsuarioDialogDados>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<UsuarioDialog, Usuario>);
  private readonly usuarioService = inject(UsuarioService);

  protected readonly editando = this.dados.usuario !== null;
  protected readonly limites = LIMITES;
  protected readonly limiteSenha = LIMITES_SENHA.max;
  protected readonly perfis = Object.entries(PERFIL_LABEL) as [PerfilUsuario, string][];
  protected readonly descricaoPerfil: Record<PerfilUsuario, string> = {
    USUARIO: 'Abre e acompanha os próprios chamados.',
    TECNICO: 'Atende chamados e vê todos os chamados.',
    ADMIN: 'Acesso total, inclusive usuários e categorias.',
  };

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mostrarSenha = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group({
    nome: [
      this.dados.usuario?.nome ?? '',
      [Validators.required, Validators.minLength(LIMITES.nomeMin), Validators.maxLength(LIMITES.nomeMax)],
    ],
    email: [this.dados.usuario?.email ?? '', [Validators.required, Validators.email, Validators.maxLength(LIMITES.emailMax)]],
    perfil: [{ value: this.dados.usuario?.perfil ?? ('USUARIO' as PerfilUsuario), disabled: this.dados.ehVoceMesmo }],
    // Na edição o campo fica desabilitado: não é validado nem enviado (a senha não muda por aqui).
    senha: [
      { value: '', disabled: this.editando },
      [Validators.required, Validators.maxLength(LIMITES_SENHA.max), senhaForte],
    ],
  });

  protected salvar(): void {
    const { nome, email } = this.form.controls;
    nome.setValue(nome.value.trim());
    email.setValue(email.value.trim());
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // getRawValue inclui o perfil mesmo quando desabilitado (a API exige o campo).
    const valores = this.form.getRawValue();
    const requisicao: Observable<Usuario> = this.dados.usuario
      ? this.usuarioService.atualizar(this.dados.usuario.id, { nome: valores.nome, email: valores.email, perfil: valores.perfil })
      : this.usuarioService.criar(valores);

    this.erro.set(null);
    this.salvando.set(true);
    this.dialogRef.disableClose = true;

    requisicao
      .pipe(
        finalize(() => {
          this.salvando.set(false);
          this.dialogRef.disableClose = false;
        }),
      )
      .subscribe({
        next: (salvo) => this.dialogRef.close(salvo),
        error: (erro: unknown) => {
          if (erro instanceof HttpErrorResponse && erro.status === 409) {
            email.setErrors({ emUso: true });
            email.markAsTouched();
          } else {
            aplicarErrosDaApi(this.form, erro);
            this.erro.set(mensagemErro(erro));
          }
        },
      });
  }
}
