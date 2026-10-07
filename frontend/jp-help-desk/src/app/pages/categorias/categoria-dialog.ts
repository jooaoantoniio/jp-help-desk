import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { finalize } from 'rxjs';
import { Categoria } from '../../models/categoria';
import { CategoriaService } from '../../services/categoria.service';
import { aplicarErrosDaApi, mensagemErro } from '../../services/erro-api';

/** Mesmos limites do CategoriaRequest da API. */
const LIMITES = { nomeMin: 2, nomeMax: 50, descricaoMax: 250 } as const;

/**
 * Cria (dados = null) ou edita uma categoria. O próprio diálogo chama a API, assim um erro
 * (ex.: nome repetido) aparece aqui dentro sem perder o que foi digitado.
 * Fecha com a categoria salva, ou undefined ao desistir.
 */
@Component({
  selector: 'app-categoria-dialog',
  imports: [
    ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatIconModule, MatProgressBarModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ categoria ? 'Editar categoria' : 'Nova categoria' }}</h2>
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
          <mat-label>Nome</mat-label>
          <input matInput formControlName="nome" [maxlength]="limites.nomeMax" cdkFocusInitial />
          <mat-hint align="end">{{ form.controls.nome.value.length }}/{{ limites.nomeMax }}</mat-hint>
          @if (form.controls.nome.hasError('required')) {
            <mat-error>Informe o nome.</mat-error>
          } @else if (form.controls.nome.hasError('minlength')) {
            <mat-error>O nome deve ter pelo menos {{ limites.nomeMin }} caracteres.</mat-error>
          } @else if (form.controls.nome.hasError('emUso')) {
            <mat-error>Já existe uma categoria com esse nome.</mat-error>
          } @else if (form.controls.nome.hasError('api')) {
            <mat-error>{{ form.controls.nome.getError('api') }}</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Descrição (opcional)</mat-label>
          <textarea matInput formControlName="descricao" rows="3" [maxlength]="limites.descricaoMax"
            placeholder="Ex.: Computadores, notebooks e periféricos"></textarea>
          <mat-hint align="end">{{ form.controls.descricao.value.length }}/{{ limites.descricaoMax }}</mat-hint>
        </mat-form-field>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close [disabled]="salvando()">Cancelar</button>
        <button mat-flat-button type="submit" [disabled]="salvando()">{{ categoria ? 'Salvar' : 'Criar categoria' }}</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    mat-dialog-content { display: flex; flex-direction: column; gap: 8px; padding-top: 8px !important; }
    .progresso { position: absolute; inset: 0 0 auto; }
  `,
})
export class CategoriaDialog {
  protected readonly categoria = inject<Categoria | null>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CategoriaDialog, Categoria>);
  private readonly categoriaService = inject(CategoriaService);

  protected readonly limites = LIMITES;
  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    nome: [
      this.categoria?.nome ?? '',
      [Validators.required, Validators.minLength(LIMITES.nomeMin), Validators.maxLength(LIMITES.nomeMax)],
    ],
    descricao: [this.categoria?.descricao ?? '', Validators.maxLength(LIMITES.descricaoMax)],
  });

  protected salvar(): void {
    // Espaços nas pontas não contam (a API também faz Trim).
    this.form.controls.nome.setValue(this.form.controls.nome.value.trim());
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valores = this.form.getRawValue();
    const dados = { nome: valores.nome, descricao: valores.descricao.trim() || null };
    const requisicao = this.categoria
      ? this.categoriaService.atualizar(this.categoria.id, dados)
      : this.categoriaService.criar(dados);

    this.erro.set(null);
    this.salvando.set(true);
    this.dialogRef.disableClose = true; // não fecha com Esc/clique fora durante o envio

    requisicao
      .pipe(
        finalize(() => {
          this.salvando.set(false);
          this.dialogRef.disableClose = false;
        }),
      )
      .subscribe({
        next: (salva) => this.dialogRef.close(salva),
        error: (erro: unknown) => {
          if (erro instanceof HttpErrorResponse && erro.status === 409) {
            // Mostra o conflito no próprio campo.
            this.form.controls.nome.setErrors({ emUso: true });
            this.form.controls.nome.markAsTouched();
          } else {
            aplicarErrosDaApi(this.form, erro);
            this.erro.set(mensagemErro(erro));
          }
        },
      });
  }
}
