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
import { PerfilBadge } from '../../components/badges/perfil-badge';
import { PaginadorIntl } from '../../components/paginacao/paginador-intl';
import { PERFIL_LABEL, PerfilUsuario } from '../../models/enums';
import { Usuario } from '../../models/usuario';
import { AuthService } from '../../services/auth.service';
import { mensagemErro } from '../../services/erro-api';
import { UsuarioService } from '../../services/usuario.service';
import { UsuarioDialog, UsuarioDialogDados } from './usuario-dialog';

type Situacao = 'todos' | 'ativos' | 'inativos';
const ATIVO_POR_SITUACAO: Record<Situacao, boolean | undefined> = { todos: undefined, ativos: true, inativos: false };

/** Gerenciamento de usuários (somente ADMIN). */
@Component({
  selector: 'app-usuarios',
  imports: [
    DatePipe, ReactiveFormsModule, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatTooltipModule, PerfilBadge,
  ],
  providers: [{ provide: MatPaginatorIntl, useClass: PaginadorIntl }],
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.scss',
})
export class Usuarios {
  private readonly auth = inject(AuthService);
  private readonly usuarioService = inject(UsuarioService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly opcoesPerfil = Object.entries(PERFIL_LABEL) as [PerfilUsuario, string][];
  protected readonly meuId = computed(() => this.auth.usuario()?.id);

  protected readonly ehMobile = toSignal(
    inject(BreakpointObserver).observe('(max-width: 959.98px)').pipe(map((estado) => estado.matches)),
    { initialValue: false },
  );
  protected readonly colunas = computed(() =>
    this.ehMobile() ? ['usuario', 'acoes'] : ['usuario', 'perfil', 'situacao', 'criadoEm', 'acoes'],
  );

  // ---------- Filtros e paginação ----------

  protected readonly busca = new FormControl('', { nonNullable: true });
  private readonly termo = toSignal(
    this.busca.valueChanges.pipe(debounceTime(300), map((texto) => texto.trim()), distinctUntilChanged()),
    { initialValue: '' },
  );
  protected readonly perfil = signal<PerfilUsuario | null>(null);
  protected readonly situacao = signal<Situacao>('todos');

  protected readonly pagina = linkedSignal({
    source: () => [this.termo(), this.perfil(), this.situacao()],
    computation: () => 1,
  });
  protected readonly tamanhoPagina = signal(10);

  protected readonly usuarios = rxResource({
    params: () => ({
      busca: this.termo(),
      perfil: this.perfil() ?? undefined,
      ativo: ATIVO_POR_SITUACAO[this.situacao()],
      pagina: this.pagina(),
      tamanhoPagina: this.tamanhoPagina(),
    }),
    stream: ({ params }) => this.usuarioService.listar(params),
  });
  protected readonly mensagemErro = computed(() => mensagemErro(this.usuarios.error()));
  protected readonly filtrando = computed(() => this.termo() !== '' || this.perfil() !== null || this.situacao() !== 'todos');

  protected paginar(evento: PageEvent): void {
    this.tamanhoPagina.set(evento.pageSize);
    this.pagina.set(evento.pageIndex + 1);
  }

  protected limparFiltros(): void {
    this.busca.setValue('');
    this.perfil.set(null);
    this.situacao.set('todos');
  }

  // ---------- Ações ----------

  protected abrirFormulario(usuario: Usuario | null = null): void {
    const dados: UsuarioDialogDados = { usuario, ehVoceMesmo: usuario?.id === this.meuId() };

    this.dialog
      .open(UsuarioDialog, { data: dados, width: '560px', maxWidth: '95vw' })
      .afterClosed()
      .pipe(filter((salvo): salvo is Usuario => salvo !== undefined))
      .subscribe((salvo) => {
        this.usuarios.reload();
        // Editou a própria conta: atualiza o nome exibido no cabeçalho.
        if (salvo.id === this.meuId()) {
          this.auth.recarregarUsuario().subscribe();
        }
        this.snackBar.open(usuario ? `Usuário "${salvo.nome}" atualizado.` : `Usuário "${salvo.nome}" criado.`, 'OK', {
          duration: 3000,
        });
      });
  }

  protected desativar(usuario: Usuario): void {
    const dados: ConfirmacaoDialogDados = {
      titulo: 'Desativar usuário',
      mensagem: `${usuario.nome} não conseguirá mais entrar no sistema (inclusive quem já estiver logado). Os chamados e o histórico dessa pessoa são mantidos e você pode reativá-la depois.`,
      confirmar: 'Desativar',
      destrutiva: true,
    };

    this.dialog
      .open(ConfirmacaoDialog, { data: dados, width: '440px', maxWidth: '95vw' })
      .afterClosed()
      .pipe(
        filter(Boolean),
        switchMap(() => this.usuarioService.desativar(usuario.id)),
      )
      .subscribe(this.aposAlterar(`Usuário "${usuario.nome}" desativado.`));
  }

  protected ativar(usuario: Usuario): void {
    this.usuarioService.ativar(usuario.id).subscribe(this.aposAlterar(`Usuário "${usuario.nome}" reativado.`));
  }

  private aposAlterar(mensagem: string) {
    return {
      next: () => {
        this.usuarios.reload();
        this.snackBar.open(mensagem, 'OK', { duration: 3000 });
      },
      error: (erro: unknown) => this.snackBar.open(mensagemErro(erro), 'OK', { duration: 6000 }),
    };
  }
}
