import { Component, computed, input } from '@angular/core';
import { classeBadge, STATUS_LABEL, StatusChamado } from '../../models/enums';

/** Badge colorido com o status do chamado. Ex.: <app-status-badge [status]="chamado.status" /> */
@Component({
  selector: 'app-status-badge',
  template: `<span [class]="classe()">{{ rotulo() }}</span>`,
})
export class StatusBadge {
  readonly status = input.required<StatusChamado>();

  protected readonly rotulo = computed(() => STATUS_LABEL[this.status()]);
  protected readonly classe = computed(() => `badge ${classeBadge('status', this.status())}`);
}
