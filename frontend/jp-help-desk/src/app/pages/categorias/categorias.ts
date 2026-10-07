import { BreakpointObserver } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, linkedSignal, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorIntl, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged, filter, map, switchMap } from 'rxjs';
import { ConfirmacaoDialog, ConfirmacaoDialogDados } from '../../components/dialogs/confirmacao-dialog';
import { PaginadorIntl } from '../../components/paginacao/paginador-intl';
import { Categoria } from '../../models/categoria';
import { CategoriaService } from '../../services/categoria.service';
import { mensagemErro } from '../../services/erro-api';
import { CategoriaDialog } from './categoria-dialog';

type Situacao = 'todas' | 'ativas' | 'inativas';
const ATIVO_POR_SITUACAO: Record<Situacao, boolean | undefined> = { todas: undefined, ativas: true, inativas: false };

/** Gerenciamento de categorias (somente ADMIN). */
@Component({
  selector: 'app-categorias',
  imports: [
    DatePipe, ReactiveFormsModule, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatTooltipModule,
  ],
  providers: [{ provide: MatPaginatorIntl, useClass: PaginadorIntl }],
  templateUrl: './categorias.html',
  styleUrl: './categorias.scss',
})
export class Categorias {
  private readonly categoriaService = inject(CategoriaService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly ehMobile = toSignal(
    inject(BreakpointObserver).observe('(max-width: 959.98px)').pipe(map((estado) => estado.matches)),
    { initialValue: false },
  );
  protected readonly colunas = computed(() =>
    this.ehMobile() ? ['nome', 'acoes'] : ['nome', 'descricao', 'situacao', 'criadoEm', 'acoes'],
  );

  // ---------- Filtros e paginação ----------

  /** Busca com debounce: só consulta a API 300 ms depois que a pessoa para de digitar. */
  protected readonly busca = new FormControl('', { nonNullable: true });
  private readonly termo = toSignal(
    this.busca.valueChanges.pipe(debounceTime(300), map((texto) => texto.trim()), distinctUntilChanged()),
    { initialValue: '' },
  );
  protected readonly situacao = signal<Situacao>('todas');

  /** linkedSignal: volta para a página 1 sempre que a busca ou a situação mudam. */
  protected readonly pagina = linkedSignal({ source: () => [this.termo(), this.situacao()], computation: () => 1 });
  protected readonly tamanhoPagina = signal(10);

  protected readonly categorias = rxResource({
    params: () => ({
      busca: this.termo(),
      ativo: ATIVO_POR_SITUACAO[this.situacao()],
      pagina: this.pagina(),
      tamanhoPagina: this.tamanhoPagina(),
    }),
    stream: ({ params }) => this.categoriaService.listar(params),
  });
  protected readonly mensagemErro = computed(() => mensagemErro(this.categorias.error()));
  protected readonly filtrando = computed(() => this.termo() !== '' || this.situacao() !== 'todas');

  protected paginar(evento: PageEvent): void {
    this.tamanhoPagina.set(evento.pageSize);
    this.pagina.set(evento.pageIndex + 1);
  }

  protected limparFiltros(): void {
    this.busca.setValue('');
    this.situacao.set('todas');
  }

  // ---------- Ações ----------

  protected abrirFormulario(categoria: Categoria | null = null): void {
    this.dialog
      .open(CategoriaDialog, { data: categoria, width: '480px', maxWidth: '95vw' })
      .afterClosed()
      .pipe(filter((salva): salva is Categoria => salva !== undefined))
      .subscribe((salva) => {
        this.categorias.reload();
        this.snackBar.open(categoria ? `Categoria "${salva.nome}" atualizada.` : `Categoria "${salva.nome}" criada.`, 'OK', {
          duration: 3000,
        });
      });
  }

  protected desativar(categoria: Categoria): void {
    const dados: ConfirmacaoDialogDados = {
      titulo: 'Desativar categoria',
      mensagem: `"${categoria.nome}" deixará de aparecer na abertura de chamados. Os chamados que já usam essa categoria não são alterados e você pode reativá-la depois.`,
      confirmar: 'Desativar',
      destrutiva: true,
    };

    this.dialog
      .open(ConfirmacaoDialog, { data: dados, width: '440px', maxWidth: '95vw' })
      .afterClosed()
      .pipe(
        filter(Boolean),
        switchMap(() => this.categoriaService.desativar(categoria.id)),
      )
      .subscribe(this.aposAlterar(`Categoria "${categoria.nome}" desativada.`));
  }

  protected ativar(categoria: Categoria): void {
    this.categoriaService.ativar(categoria.id).subscribe(this.aposAlterar(`Categoria "${categoria.nome}" reativada.`));
  }

  private aposAlterar(mensagem: string) {
    return {
      next: () => {
        this.categorias.reload();
        this.snackBar.open(mensagem, 'OK', { duration: 3000 });
      },
      error: (erro: unknown) => this.snackBar.open(mensagemErro(erro), 'OK', { duration: 6000 }),
    };
  }
}
