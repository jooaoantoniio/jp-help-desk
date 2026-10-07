import { Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { nivelSenha, REQUISITOS_SENHA } from '../../validacao/senha';

const ROTULO_NIVEL = { vazia: '', fraca: 'Fraca', media: 'Média', forte: 'Forte' } as const;

/** Barra de força e checklist dos requisitos da senha. Ex.: <app-forca-senha [senha]="campo.value" /> */
@Component({
  selector: 'app-forca-senha',
  imports: [MatIconModule],
  template: `
    <div class="barra" [attr.data-nivel]="nivel()" aria-hidden="true">
      <span></span><span></span><span></span>
    </div>
    <p class="nivel" aria-live="polite">
      @if (rotuloNivel()) {
        Força da senha: <strong>{{ rotuloNivel() }}</strong>
      }
    </p>
    <ul class="requisitos">
      @for (requisito of requisitos(); track requisito.rotulo) {
        <li [class.ok]="requisito.ok">
          <mat-icon aria-hidden="true">{{ requisito.ok ? 'check_circle' : 'radio_button_unchecked' }}</mat-icon>
          {{ requisito.rotulo }}
          <span class="sr-only">{{ requisito.ok ? '(atendido)' : '(pendente)' }}</span>
        </li>
      }
    </ul>
  `,
  styles: `
    :host { display: block; margin: -4px 0 8px; font-size: 13px; color: var(--jp-text-muted); }
    .barra { display: grid; grid-template-columns: repeat(3, 1fr); gap: 4px; }
    .barra span { height: 4px; border-radius: 2px; background: var(--jp-border); transition: background 0.2s; }
    .barra[data-nivel='fraca'] span:nth-child(1) { background: var(--jp-status-cancelado-fg); }
    .barra[data-nivel='media'] span:nth-child(-n + 2) { background: var(--jp-prioridade-alta-fg); }
    .barra[data-nivel='forte'] span { background: var(--jp-status-resolvido-fg); }
    .nivel { min-height: 20px; margin: 4px 0; }
    .requisitos { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 2px 12px; margin: 0; padding: 0; list-style: none; }
    li { display: flex; align-items: center; gap: 6px; }
    li.ok { color: var(--jp-status-resolvido-fg); }
    mat-icon { width: 16px; height: 16px; font-size: 16px; }
    .sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }
  `,
})
export class ForcaSenha {
  readonly senha = input.required<string>();

  protected readonly nivel = computed(() => nivelSenha(this.senha()));
  protected readonly rotuloNivel = computed(() => ROTULO_NIVEL[this.nivel()]);
  protected readonly requisitos = computed(() =>
    REQUISITOS_SENHA.map((requisito) => ({ rotulo: requisito.rotulo, ok: requisito.atende(this.senha()) })),
  );
}
