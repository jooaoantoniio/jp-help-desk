import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { PERFIL_LABEL } from '../../models/enums';
import { UsuarioService } from '../../services/usuario.service';

export interface AtribuirDialogDados {
  tecnicoAtualId: number | null;
}

/** ADMIN escolhe o técnico responsável. Fecha com o ID escolhido, ou undefined ao desistir. */
@Component({
  selector: 'app-atribuir-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatSelectModule],
  template: `
    <h2 mat-dialog-title>Atribuir técnico</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="campo">
        <mat-label>Técnico responsável</mat-label>
        <mat-select [formControl]="tecnicoId">
          @for (usuario of equipe(); track usuario.id) {
            <mat-option [value]="usuario.id">{{ usuario.nome }} ({{ perfis[usuario.perfil] }})</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Voltar</button>
      <button mat-flat-button type="button" [disabled]="tecnicoId.invalid" (click)="confirmar()">Atribuir</button>
    </mat-dialog-actions>
  `,
  styles: `.campo { width: 100%; min-width: 280px; }`,
})
export class AtribuirDialog {
  private readonly dados = inject<AtribuirDialogDados>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AtribuirDialog, number>);

  protected readonly perfis = PERFIL_LABEL;
  protected readonly equipe = toSignal(inject(UsuarioService).listarEquipe(), { initialValue: [] });
  protected readonly tecnicoId = new FormControl<number | null>(this.dados.tecnicoAtualId, Validators.required);

  protected confirmar(): void {
    this.dialogRef.close(this.tecnicoId.value!);
  }
}
