import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

export interface ConfirmacaoDialogDados {
  titulo: string;
  mensagem: string;
  /** Texto do botão de confirmação. Ex.: "Desativar". */
  confirmar: string;
  /** Ação destrutiva recebe destaque de alerta. */
  destrutiva?: boolean;
}

/** Pergunta simples de sim/não. Fecha com true ao confirmar, ou undefined ao desistir. */
@Component({
  selector: 'app-confirmacao-dialog',
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ dados.titulo }}</h2>
    <mat-dialog-content>
      <p>{{ dados.mensagem }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Voltar</button>
      <button mat-flat-button type="button" [class.destrutiva]="dados.destrutiva" [mat-dialog-close]="true">
        {{ dados.confirmar }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `.destrutiva { --mat-button-filled-container-color: var(--jp-status-cancelado-fg); }`,
})
export class ConfirmacaoDialog {
  protected readonly dados = inject<ConfirmacaoDialogDados>(MAT_DIALOG_DATA);
}
