import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ChamadoRequest } from '../../models/chamado';
import { PRIORIDADE_LABEL, PrioridadeChamado } from '../../models/enums';
import { AuthService } from '../../services/auth.service';
import { CategoriaService } from '../../services/categoria.service';
import { ChamadoService } from '../../services/chamado.service';
import { mensagemErro } from '../../services/erro-api';

/** Limites iguais aos da API (ChamadoRequest no backend). */
export const LIMITES = { tituloMin: 5, tituloMax: 150, descricaoMin: 10, descricaoMax: 4000 } as const;

/** Abertura (/chamados/novo) e edição (/chamados/:id/editar) de chamados. */
@Component({
  selector: 'app-chamado-form',
  imports: [
    ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatProgressBarModule,
  ],
  templateUrl: './chamado-form.html',
  styleUrl: './chamado-form.scss',
})
export class ChamadoForm {
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly auth = inject(AuthService);
  private readonly chamadoService = inject(ChamadoService);

  /** Parâmetro ":id" da rota (ausente em /chamados/novo). */
  readonly id = input<string>();
  protected readonly editando = computed(() => this.id() !== undefined);
  protected readonly limites = LIMITES;

  /** Chamado em edição (o recurso fica ocioso ao abrir um novo chamado). */
  protected readonly chamado = rxResource({
    params: () => (this.editando() ? Number(this.id()) : undefined),
    stream: ({ params }) => this.chamadoService.obter(params),
  });

  private readonly categoriasAtivas = toSignal(inject(CategoriaService).listarAtivas(), { initialValue: [] });

  /** Inclui a categoria atual do chamado mesmo que tenha sido desativada depois. */
  protected readonly categorias = computed(() => {
    const ativas = this.categoriasAtivas().map((c) => ({ id: c.id, nome: c.nome }));
    const atual = this.chamado.value()?.categoria;
    return atual && !ativas.some((c) => c.id === atual.id) ? [...ativas, { id: atual.id, nome: `${atual.nome} (inativa)` }] : ativas;
  });

  protected readonly prioridades = Object.entries(PRIORIDADE_LABEL) as [PrioridadeChamado, string][];

  /** Motivo pelo qual o chamado não pode ser editado (null = pode editar). */
  protected readonly bloqueio = computed(() => {
    const chamado = this.chamado.value();
    if (!chamado) {
      return null;
    }
    if (chamado.status === 'FECHADO' || chamado.status === 'CANCELADO') {
      return 'Chamados fechados ou cancelados não podem ser editados.';
    }
    if (this.auth.temPerfil('USUARIO') && chamado.status !== 'ABERTO') {
      return 'Você só pode editar seu chamado enquanto ele estiver aberto (antes do atendimento começar).';
    }
    return null;
  });

  protected readonly form = inject(NonNullableFormBuilder).group({
    titulo: ['', [Validators.required, Validators.minLength(LIMITES.tituloMin), Validators.maxLength(LIMITES.tituloMax)]],
    descricao: ['', [Validators.required, Validators.minLength(LIMITES.descricaoMin), Validators.maxLength(LIMITES.descricaoMax)]],
    prioridade: ['MEDIA' as PrioridadeChamado, Validators.required],
    categoriaId: [null as number | null, Validators.required],
  });

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly erroCarregamento = computed(() => (this.chamado.error() ? mensagemErro(this.chamado.error()) : null));

  constructor() {
    // Ao carregar o chamado em edição, preenche o formulário (e bloqueia se não puder editar).
    effect(() => {
      const chamado = this.chamado.value();
      if (chamado) {
        this.form.reset({
          titulo: chamado.titulo,
          descricao: chamado.descricao,
          prioridade: chamado.prioridade,
          categoriaId: chamado.categoria.id,
        });
      }
      if (this.bloqueio()) {
        this.form.disable();
      } else {
        this.form.enable();
      }
    });
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valores = this.form.getRawValue();
    const dados: ChamadoRequest = {
      titulo: valores.titulo.trim(),
      descricao: valores.descricao.trim(),
      prioridade: valores.prioridade,
      categoriaId: valores.categoriaId!,
    };

    this.salvando.set(true);
    this.erro.set(null);

    const requisicao = this.editando()
      ? this.chamadoService.atualizar(Number(this.id()), dados)
      : this.chamadoService.abrir(dados);

    requisicao.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (chamado) => {
        const acao = this.editando() ? 'atualizado' : 'aberto';
        this.snackBar.open(`Chamado #${chamado.id} ${acao} com sucesso.`, 'OK', { duration: 4000 });
        void this.router.navigate(['/chamados', chamado.id]);
      },
      error: (erro: unknown) => this.erro.set(mensagemErro(erro)),
    });
  }

  protected voltar(): string[] {
    return this.editando() ? ['/chamados', this.id()!] : ['/chamados'];
  }
}
