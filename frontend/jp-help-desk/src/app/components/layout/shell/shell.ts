import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { Header } from '../header/header';
import { Sidebar } from '../sidebar/sidebar';

/**
 * Estrutura das páginas autenticadas: sidebar + header + conteúdo da rota.
 * Desktop: sidebar fixa. Mobile (< 960px): sidebar sobreposta, aberta pelo botão de menu.
 */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, MatSidenavModule, Sidebar, Header],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly breakpointObserver = inject(BreakpointObserver);

  /** true em telas menores que 960px. Observable do CDK convertido em signal. */
  protected readonly ehMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 959.98px)').pipe(map((estado) => estado.matches)),
    { initialValue: false },
  );

  /** Estado do menu no mobile (no desktop a sidebar fica sempre aberta). */
  protected readonly menuMobileAberto = signal(false);

  protected alternarMenu(): void {
    this.menuMobileAberto.update((aberto) => !aberto);
  }

  protected fecharMenuMobile(): void {
    this.menuMobileAberto.set(false);
  }
}
