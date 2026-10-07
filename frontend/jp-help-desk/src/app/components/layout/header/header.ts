import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';

/** Barra superior: botão de menu (mobile), ação "Novo chamado" e menu do usuário. */
@Component({
  selector: 'app-header',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  /** Exibe o botão que abre a sidebar (somente no mobile). */
  readonly mostrarBotaoMenu = input(false);

  readonly alternarMenu = output<void>();
}
