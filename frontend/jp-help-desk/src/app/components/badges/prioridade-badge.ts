import { Component, computed, input } from '@angular/core';
import { classeBadge, PRIORIDADE_LABEL, PrioridadeChamado } from '../../models/enums';

/** Badge colorido com a prioridade do chamado. Ex.: <app-prioridade-badge [prioridade]="chamado.prioridade" /> */
@Component({
  selector: 'app-prioridade-badge',
  template: `<span [class]="classe()">{{ rotulo() }}</span>`,
})
export class PrioridadeBadge {
  readonly prioridade = input.required<PrioridadeChamado>();

  protected readonly rotulo = computed(() => PRIORIDADE_LABEL[this.prioridade()]);
  protected readonly classe = computed(() => `badge ${classeBadge('prioridade', this.prioridade())}`);
}
