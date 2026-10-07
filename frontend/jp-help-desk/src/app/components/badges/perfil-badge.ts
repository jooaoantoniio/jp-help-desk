import { Component, computed, input } from '@angular/core';
import { classeBadge, PERFIL_LABEL, PerfilUsuario } from '../../models/enums';

/** Badge colorido com o perfil do usuário. Ex.: <app-perfil-badge [perfil]="usuario.perfil" /> */
@Component({
  selector: 'app-perfil-badge',
  template: `<span [class]="classe()">{{ rotulo() }}</span>`,
})
export class PerfilBadge {
  readonly perfil = input.required<PerfilUsuario>();

  protected readonly rotulo = computed(() => PERFIL_LABEL[this.perfil()]);
  protected readonly classe = computed(() => `badge ${classeBadge('perfil', this.perfil())}`);
}
