import { BreakpointObserver } from '@angular/cdk/layout';
import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorIntl, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map, of } from 'rxjs';
import { PrioridadeBadge } from '../../components/badges/prioridade-badge';
import { StatusBadge } from '../../components/badges/status-badge';
import { PaginadorIntl } from '../../components/paginacao/paginador-intl';
import { OrdenacaoChamado, PRIORIDADE_LABEL, PrioridadeChamado, STATUS_LABEL, StatusChamado } from '../../models/enums';
import { AuthService } from '../../services/auth.service';
import { CategoriaService } from '../../services/categoria.service';
import { ChamadoService } from '../../services/chamado.service';
import { mensagemErro } from '../../services/erro-api';
import { UsuarioService } from '../../services/usuario.service';
import { contarFiltros, filtroParaApi, filtroParaUrl, FiltroTela, lerFiltroDaUrl, ORDENACAO_PADRAO } from './filtro-url';

/** Coluna da tabela -> campo de ordenação da API. */
const COLUNAS_ORDENAVEIS: Record<string, OrdenacaoChamado> = {
  id: 'ID',
  titulo: 'TITULO',
  status: 'STATUS',
  prioridade: 'PRIORIDADE',
  dataAbertura: 'DATA_ABERTURA',
};

@Component({
  selector: 'app-chamados-lista',
  imports: [
    DatePipe, RouterLink, ReactiveFormsModule, MatTableModule, MatSortModule, MatPaginatorModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatCheckboxModule, MatButtonModule,
    MatIconModule, MatProgressBarModule, StatusBadge, PrioridadeBadge,
  ],
  providers: [{ provide: MatPaginatorIntl, useClass: PaginadorIntl }],
  templateUrl: './chamados-lista.html',
  styleUrl: './chamados-lista.scss',
})
export class ChamadosLista {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly chamadoService = inject(ChamadoService);

  protected readonly ehAdmin = computed(() => this.auth.temPerfil('ADMIN'));
  protected readonly ehTecnico = computed(() => this.auth.temPerfil('TECNICO'));
  protected readonly ehUsuario = computed(() => this.auth.temPerfil('USUARIO'));

  protected readonly ehMobile = toSignal(
    inject(BreakpointObserver).observe('(max-width: 959.98px)').pipe(map((estado) => estado.matches)),
    { initialValue: false },
  );

  // ---------- Estado: a URL é a fonte da verdade ----------

  private readonly queryParams = toSignal(this.route.queryParamMap, { requireSync: true });
  protected readonly filtro = computed(() => lerFiltroDaUrl(this.queryParams()));
  protected readonly totalFiltros = computed(() => contarFiltros(this.filtro()));

  /** Refaz a busca automaticamente sempre que o filtro (URL) muda. */
  protected readonly chamados = rxResource({
    params: () => filtroParaApi(this.filtro()),
    stream: ({ params }) => this.chamadoService.listar(params),
  });
  protected readonly mensagemErro = computed(() => mensagemErro(this.chamados.error()));

  // ---------- Opções dos filtros ----------

  protected readonly opcoesStatus = Object.entries(STATUS_LABEL) as [StatusChamado, string][];
  protected readonly opcoesPrioridade = Object.entries(PRIORIDADE_LABEL) as [PrioridadeChamado, string][];
  protected readonly categorias = toSignal(inject(CategoriaService).listarAtivas(), { initialValue: [] });

  /** Lista de usuários só para o ADMIN (somente ele pode consultar /api/usuarios). */
  private readonly usuarioService = inject(UsuarioService);
  protected readonly usuarios = toSignal(this.ehAdmin() ? this.usuarioService.listarAtivos() : of([]), { initialValue: [] });
  protected readonly equipe = computed(() => this.usuarios().filter((u) => u.perfil !== 'USUARIO'));

  // ---------- Formulário de filtros ----------

  protected readonly filtrosAbertos = signal(false);
  protected readonly form = inject(NonNullableFormBuilder).group({
    titulo: [''],
    id: [null as number | null],
    status: [null as StatusChamado | null],
    prioridade: [null as PrioridadeChamado | null],
    categoriaId: [null as number | null],
    solicitanteId: [null as number | null],
    tecnicoId: [null as number | null],
    atribuidosAMim: [false],
    de: [''],
    ate: [''],
  });

  protected readonly colunas = ['id', 'titulo', 'status', 'prioridade', 'categoria', 'solicitante', 'tecnico', 'dataAbertura'];

  constructor() {
    // Mantém o formulário sincronizado com a URL (ex.: link do dashboard, botão Voltar).
    effect(() => {
      const filtro = this.filtro();
      this.form.reset(
        {
          ...filtro,
          atribuidosAMim: filtro.tecnicoId !== null && filtro.tecnicoId === this.auth.usuario()?.id,
        },
        { emitEvent: false },
      );
    });

    // Abre o painel de filtros se a página já chegar filtrada (exceto busca por título).
    if (this.totalFiltros() > (this.filtro().titulo ? 1 : 0)) {
      this.filtrosAbertos.set(true);
    }
  }

  protected filtrar(): void {
    const valores = this.form.getRawValue();
    const tecnicoId = this.ehTecnico() ? (valores.atribuidosAMim ? (this.auth.usuario()?.id ?? null) : null) : valores.tecnicoId;
    this.navegar({ ...this.filtro(), ...valores, tecnicoId, pagina: 1 });
  }

  protected limpar(): void {
    this.navegar({ ordenarPor: this.filtro().ordenarPor, decrescente: this.filtro().decrescente });
  }

  protected paginar(evento: PageEvent): void {
    this.navegar({ ...this.filtro(), pagina: evento.pageIndex + 1, tamanhoPagina: evento.pageSize });
  }

  protected ordenar(evento: Sort): void {
    const semOrdenacao = evento.direction === '';
    this.navegar({
      ...this.filtro(),
      ordenarPor: semOrdenacao ? ORDENACAO_PADRAO : COLUNAS_ORDENAVEIS[evento.active],
      decrescente: semOrdenacao || evento.direction === 'desc',
      pagina: 1,
    });
  }

  /** Coluna ativa na tabela a partir do filtro (inverso de COLUNAS_ORDENAVEIS). */
  protected colunaOrdenada(): string {
    return Object.keys(COLUNAS_ORDENAVEIS).find((coluna) => COLUNAS_ORDENAVEIS[coluna] === this.filtro().ordenarPor) ?? 'dataAbertura';
  }

  private navegar(filtro: Partial<FiltroTela>): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams: filtroParaUrl(filtro) });
  }
}
