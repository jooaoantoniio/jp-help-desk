import { DatePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Params, RouterLink } from '@angular/router';
import { PrioridadeBadge } from '../../components/badges/prioridade-badge';
import { StatusBadge } from '../../components/badges/status-badge';
import { GraficoBarras, ItemGrafico } from '../../components/grafico-barras/grafico-barras';
import { classeBadge, PRIORIDADE_LABEL, STATUS_LABEL } from '../../models/enums';
import { AuthService } from '../../services/auth.service';
import { DashboardService } from '../../services/dashboard.service';
import { mensagemErro } from '../../services/erro-api';

interface CardIndicador {
  titulo: string;
  valor: number;
  icone: string;
  cor: string;
  filtro: Params;
}

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, StatusBadge, PrioridadeBadge, GraficoBarras],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private readonly auth = inject(AuthService);

  protected readonly dashboard = inject(DashboardService).carregar();

  protected readonly primeiroNome = computed(() => this.auth.usuario()?.nome.split(' ')[0] ?? '');
  protected readonly ehEquipe = computed(() => this.auth.temPerfil('ADMIN', 'TECNICO'));
  protected readonly mensagemErro = computed(() => mensagemErro(this.dashboard.error()));

  protected readonly cards = computed<CardIndicador[]>(() => {
    const resumo = this.dashboard.value()?.resumo;
    if (!resumo) {
      return [];
    }
    return [
      { titulo: 'Total de chamados', valor: resumo.total, icone: 'confirmation_number', cor: 'var(--jp-primary)', filtro: {} },
      { titulo: 'Abertos', valor: resumo.abertos, icone: 'inbox', cor: 'var(--jp-status-aberto-fg)', filtro: { status: 'ABERTO' } },
      { titulo: 'Em atendimento', valor: resumo.emAtendimento, icone: 'support_agent', cor: 'var(--jp-status-em-atendimento-fg)', filtro: { status: 'EM_ATENDIMENTO' } },
      { titulo: 'Críticos em aberto', valor: resumo.criticosEmAberto, icone: 'priority_high', cor: 'var(--jp-prioridade-critica-fg)', filtro: { prioridade: 'CRITICA' } },
      { titulo: 'Resolvidos', valor: resumo.resolvidos, icone: 'task_alt', cor: 'var(--jp-status-resolvido-fg)', filtro: { status: 'RESOLVIDO' } },
    ];
  });

  protected readonly graficoStatus = computed<ItemGrafico[]>(() =>
    (this.dashboard.value()?.porStatus ?? []).map((item) => ({
      rotulo: STATUS_LABEL[item.status],
      valor: item.quantidade,
      cor: `var(--jp-${classeBadge('status', item.status)}-fg)`,
      filtro: { status: item.status },
    })),
  );

  protected readonly graficoPrioridade = computed<ItemGrafico[]>(() =>
    (this.dashboard.value()?.porPrioridade ?? []).map((item) => ({
      rotulo: PRIORIDADE_LABEL[item.prioridade],
      valor: item.quantidade,
      cor: `var(--jp-${classeBadge('prioridade', item.prioridade)}-fg)`,
      filtro: { prioridade: item.prioridade },
    })),
  );

  protected readonly graficoCategoria = computed<ItemGrafico[]>(() =>
    (this.dashboard.value()?.porCategoria ?? []).map((item) => ({
      rotulo: item.categoria.nome,
      valor: item.quantidade,
      cor: 'var(--jp-primary)',
      filtro: { categoriaId: item.categoria.id },
    })),
  );
}
