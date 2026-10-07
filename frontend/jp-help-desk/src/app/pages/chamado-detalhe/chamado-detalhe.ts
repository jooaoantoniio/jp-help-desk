import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { filter, finalize, Observable, switchMap } from 'rxjs';
import { PrioridadeBadge } from '../../components/badges/prioridade-badge';
import { StatusBadge } from '../../components/badges/status-badge';
import { AlterarStatusDialog, AlterarStatusDialogDados } from '../../components/dialogs/alterar-status-dialog';
import { AtribuirDialog, AtribuirDialogDados } from '../../components/dialogs/atribuir-dialog';
import { Chamado } from '../../models/chamado';
import { STATUS_LABEL, TIPO_HISTORICO_LABEL } from '../../models/enums';
import { AuthService } from '../../services/auth.service';
import { ChamadoService } from '../../services/chamado.service';
import { mensagemErro } from '../../services/erro-api';
import { AcaoStatus, acoesDeStatus, ehFinal, ICONE_HISTORICO, permiteAtribuicao } from './acoes-chamado';

@Component({
  selector: 'app-chamado-detalhe',
  imports: [
    DatePipe, RouterLink, ReactiveFormsModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatProgressBarModule, MatTooltipModule, StatusBadge, PrioridadeBadge,
  ],
  templateUrl: './chamado-detalhe.html',
  styleUrl: './chamado-detalhe.scss',
})
export class ChamadoDetalhe {
  private readonly auth = inject(AuthService);
  private readonly chamadoService = inject(ChamadoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  /** Parâmetro ":id" da rota. */
  readonly id = input.required<string>();
  private readonly chamadoId = computed(() => Number(this.id()));

  protected readonly chamado = rxResource({
    params: () => this.chamadoId(),
    stream: ({ params }) => this.chamadoService.obter(params),
  });
  protected readonly historico = rxResource({
    params: () => this.chamadoId(),
    stream: ({ params }) => this.chamadoService.historico(params),
  });

  protected readonly naoEncontrado = computed(() => {
    const erro = this.chamado.error();
    return erro instanceof HttpErrorResponse && erro.status === 404;
  });
  protected readonly erroCarregamento = computed(() => mensagemErro(this.chamado.error()));

  protected readonly rotulosHistorico = TIPO_HISTORICO_LABEL;
  protected readonly iconesHistorico = ICONE_HISTORICO;
  protected readonly processando = signal(false);

  // ---------- O que o usuário logado pode fazer neste chamado ----------

  private readonly ehEquipe = computed(() => this.auth.temPerfil('ADMIN', 'TECNICO'));
  protected readonly acoes = computed(() => (this.chamado.value() ? acoesDeStatus(this.chamado.value()!) : []));

  protected readonly podeEditar = computed(() => {
    const c = this.chamado.value();
    return !!c && !ehFinal(c.status) && (this.ehEquipe() || (c.solicitante.id === this.auth.usuario()?.id && c.status === 'ABERTO'));
  });
  protected readonly podeAssumir = computed(() => {
    const c = this.chamado.value();
    return !!c && this.ehEquipe() && permiteAtribuicao(c.status) && c.tecnico?.id !== this.auth.usuario()?.id;
  });
  protected readonly podeAtribuir = computed(() => {
    const c = this.chamado.value();
    return !!c && this.auth.temPerfil('ADMIN') && permiteAtribuicao(c.status);
  });
  protected readonly podeComentar = computed(() => {
    const c = this.chamado.value();
    return !!c && !ehFinal(c.status);
  });

  // FormGroup (e não um FormControl solto): o [formGroup] no <form> é o que faz o Angular
  // tratar o envio (ngSubmit) e impedir o submit nativo do navegador, que recarregaria a página.
  protected readonly formComentario = new FormGroup({
    texto: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(1000)],
    }),
  });
  protected readonly comentario = this.formComentario.controls.texto;

  // ---------- Ações ----------

  protected alterarStatus(acao: AcaoStatus): void {
    const dados: AlterarStatusDialogDados = {
      titulo: acao.rotulo,
      descricao: acao.descricao,
      confirmar: acao.rotulo,
      destrutiva: acao.destrutiva,
    };

    this.dialog
      .open(AlterarStatusDialog, { data: dados, width: '480px', maxWidth: '95vw' })
      .afterClosed()
      .pipe(
        filter((observacao): observacao is string => observacao !== undefined),
        switchMap((observacao) =>
          this.chamadoService.alterarStatus(this.chamadoId(), { status: acao.status, observacao: observacao || null }),
        ),
      )
      .subscribe(this.aposAlterar(`Status alterado para "${STATUS_LABEL[acao.status]}".`));
  }

  protected assumir(): void {
    this.executar(this.chamadoService.assumir(this.chamadoId()), 'Você assumiu este chamado.');
  }

  protected atribuir(): void {
    const dados: AtribuirDialogDados = { tecnicoAtualId: this.chamado.value()?.tecnico?.id ?? null };

    this.dialog
      .open(AtribuirDialog, { data: dados, maxWidth: '95vw' })
      .afterClosed()
      .pipe(
        filter((tecnicoId): tecnicoId is number => tecnicoId !== undefined),
        switchMap((tecnicoId) => this.chamadoService.atribuir(this.chamadoId(), tecnicoId)),
      )
      .subscribe(this.aposAlterar('Técnico atribuído.'));
  }

  protected comentar(formDiretiva: FormGroupDirective): void {
    if (this.comentario.invalid) {
      this.comentario.markAsTouched();
      return;
    }

    this.processando.set(true);
    this.chamadoService
      .comentar(this.chamadoId(), this.comentario.value.trim())
      .pipe(finalize(() => this.processando.set(false)))
      .subscribe({
        next: () => {
          // resetForm (e não reset): limpa também o estado "enviado", senão o campo vazio aparece com erro.
          formDiretiva.resetForm();
          this.historico.reload();
          this.chamado.reload();
          this.snackBar.open('Comentário adicionado.', 'OK', { duration: 3000 });
        },
        error: (erro: unknown) => this.avisarErro(erro),
      });
  }

  private executar(requisicao: Observable<Chamado>, mensagem: string): void {
    this.processando.set(true);
    requisicao.pipe(finalize(() => this.processando.set(false))).subscribe(this.aposAlterar(mensagem));
  }

  /** Após uma alteração: atualiza o chamado com a resposta da API e recarrega o histórico. */
  private aposAlterar(mensagem: string) {
    return {
      next: (chamado: Chamado) => {
        this.chamado.set(chamado);
        this.historico.reload();
        this.snackBar.open(mensagem, 'OK', { duration: 3000 });
      },
      error: (erro: unknown) => this.avisarErro(erro),
    };
  }

  private avisarErro(erro: unknown): void {
    this.snackBar.open(mensagemErro(erro), 'OK', { duration: 6000 });
  }
}
