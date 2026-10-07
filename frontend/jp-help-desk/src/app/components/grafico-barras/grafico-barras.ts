import { Component, computed, input } from '@angular/core';
import { Params, RouterLink } from '@angular/router';

export interface ItemGrafico {
  rotulo: string;
  valor: number;
  /** Cor da barra (qualquer valor CSS, ex.: "var(--jp-primary)"). */
  cor: string;
  /** Filtros aplicados na lista de chamados ao clicar na barra. */
  filtro?: Params;
}

/**
 * Gráfico de barras horizontais em HTML/CSS (sem biblioteca de gráficos).
 * Cada barra é proporcional ao maior valor; mostra quantidade e percentual do total.
 */
@Component({
  selector: 'app-grafico-barras',
  imports: [RouterLink],
  templateUrl: './grafico-barras.html',
  styleUrl: './grafico-barras.scss',
})
export class GraficoBarras {
  readonly itens = input.required<ItemGrafico[]>();

  private readonly total = computed(() => this.itens().reduce((soma, item) => soma + item.valor, 0));
  private readonly maior = computed(() => Math.max(1, ...this.itens().map((item) => item.valor)));

  protected readonly barras = computed(() =>
    this.itens().map((item) => ({
      ...item,
      largura: (item.valor / this.maior()) * 100,
      percentual: this.total() ? Math.round((item.valor / this.total()) * 100) : 0,
    })),
  );
}
