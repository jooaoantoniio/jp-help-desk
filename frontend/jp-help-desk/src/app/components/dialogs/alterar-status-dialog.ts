import { Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface AlterarStatusDialogDados {
  titulo: string;
  descricao: string;
  /** Ex.: "Marcar como resolvido". */
  confirmar: string;
  /** Ação destrutiva (ex.: cancelar) recebe destaque de alerta. */
  destrutiva?: boolean;
}

/**
 * Confirma uma mudança de status com observação opcional (registrada no histórico).
 * Fecha com a observação (string, possivelmente vazia) ao confirmar, ou undefined ao desistir.
 */
@Component({
  selector: 'app-alterar-status-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2 mat-dialog-title>{{ dados.titulo }}</h2>
    <mat-dialog-content>
      <p>{{ dados.descricao }}</p>
      <mat-form-field appearance="outline" class="campo">
        <mat-label>Observação (opcional)</mat-label>
        <textarea matInput [formControl]="observacao" rows="3" maxlength="500"
          placeholder="Ex.: solução aplicada, informação pendente..."></textarea>
        <mat-hint align="end">{{ observacao.value.length }}/500</mat-hint>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Voltar</button>
      <button mat-flat-button type="button" [class.destrutiva]="dados.destrutiva" (click)="confirmar()">
        {{ dados.confirmar }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .campo { width: 100%; }
    .destrutiva { --mat-button-filled-container-color: var(--jp-status-cancelado-fg); }
  `,
})
export class AlterarStatusDialog {
  protected readonly dados = inject<AlterarStatusDialogDados>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AlterarStatusDialog, string>);

  protected readonly observacao = new FormControl('', { nonNullable: true, validators: Validators.maxLength(500) });

  protected confirmar(): void {
    this.dialogRef.close(this.observacao.value.trim());
  }
}
