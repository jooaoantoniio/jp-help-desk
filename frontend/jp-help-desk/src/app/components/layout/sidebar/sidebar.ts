import { Component, computed, inject, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PerfilUsuario } from '../../../models/enums';
import { AuthService } from '../../../services/auth.service';

interface ItemMenu {
  rotulo: string;
  icone: string;
  rota: string;
  /** Perfis que veem o item. Ausente = todos. */
  perfis?: PerfilUsuario[];
}

const ITENS: ItemMenu[] = [
  { rotulo: 'Dashboard', icone: 'dashboard', rota: '/dashboard' },
  { rotulo: 'Chamados', icone: 'confirmation_number', rota: '/chamados' },
  { rotulo: 'Usuários', icone: 'group', rota: '/usuarios', perfis: ['ADMIN'] },
  { rotulo: 'Categorias', icone: 'category', rota: '/categorias', perfis: ['ADMIN'] },
  { rotulo: 'Meu perfil', icone: 'person', rota: '/perfil' },
];

/** Menu lateral de navegação, filtrado pelo perfil do usuário logado. */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  private readonly auth = inject(AuthService);

  /** Emitido ao clicar em um item (o shell fecha o menu no mobile). */
  readonly navegou = output<void>();

  /** Recalculado automaticamente quando o usuário (signal) muda. */
  protected readonly itens = computed(() =>
    ITENS.filter((item) => !item.perfis || this.auth.temPerfil(...item.perfis)),
  );
}
