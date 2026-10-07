import { Component, computed, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { PERFIL_LABEL } from '../../../models/enums';
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

  /** Iniciais para o avatar (ex.: "Técnico de Suporte" -> "TS"). */
  protected readonly iniciais = computed(() => {
    const partes = (this.usuario()?.nome ?? '').trim().split(/\s+/).filter(Boolean);
    const primeira = partes.at(0)?.[0] ?? '';
    const ultima = partes.length > 1 ? (partes.at(-1)?.[0] ?? '') : '';
    return (primeira + ultima).toUpperCase();
  });

  protected readonly perfil = computed(() => {
    const perfil = this.usuario()?.perfil;
    return perfil ? PERFIL_LABEL[perfil] : '';
  });

  protected sair(): void {
    this.auth.logout();
  }
}
