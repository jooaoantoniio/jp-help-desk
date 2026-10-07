import { Component, computed, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { PERFIL_LABEL } from '../../../models/enums';
import { iniciais } from '../../../models/usuario';
import { AuthService } from '../../../services/auth.service';

/** Barra superior: botão de menu (mobile), ação "Novo chamado" e menu do usuário logado. */
@Component({
  selector: 'app-header',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  private readonly auth = inject(AuthService);

  /** Exibe o botão que abre a sidebar (somente no mobile). */
  readonly mostrarBotaoMenu = input(false);

  readonly alternarMenu = output<void>();

  protected readonly usuario = this.auth.usuario;

  protected readonly iniciais = computed(() => iniciais(this.usuario()?.nome ?? ''));

  protected readonly perfil = computed(() => {
    const perfil = this.usuario()?.perfil;
    return perfil ? PERFIL_LABEL[perfil] : '';
  });

  protected sair(): void {
    this.auth.logout();
  }
}
