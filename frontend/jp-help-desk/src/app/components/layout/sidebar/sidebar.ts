import { Component, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PerfilUsuario } from '../../../models/enums';

interface ItemMenu {
  rotulo: string;
  icone: string;
  rota: string;
  /** Perfis que veem o item. Ausente = todos. (Filtragem aplicada na Fase 12.) */
  perfis?: PerfilUsuario[];
}

/** Menu lateral de navegação. */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  /** Emitido ao clicar em um item (o shell fecha o menu no mobile). */
  readonly navegou = output<void>();

  protected readonly itens: ItemMenu[] = [
    { rotulo: 'Dashboard', icone: 'dashboard', rota: '/dashboard' },
    { rotulo: 'Chamados', icone: 'confirmation_number', rota: '/chamados' },
    { rotulo: 'Usuários', icone: 'group', rota: '/usuarios', perfis: ['ADMIN'] },
    { rotulo: 'Categorias', icone: 'category', rota: '/categorias', perfis: ['ADMIN'] },
    { rotulo: 'Meu perfil', icone: 'person', rota: '/perfil' },
  ];
}
