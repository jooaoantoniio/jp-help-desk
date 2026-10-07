import { Chamado } from '../../models/chamado';
import { acoesDeStatus } from './acoes-chamado';

const chamadoBase: Chamado = {
  id: 1,
  titulo: 'Teste',
  descricao: 'Descrição de teste',
  status: 'ABERTO',
  prioridade: 'MEDIA',
  categoria: { id: 1, nome: 'Hardware' },
  solicitante: { id: 3, nome: 'Usuário Padrão' },
  tecnico: null,
  dataAbertura: '2026-10-07T00:00:00Z',
  dataAtualizacao: null,
  dataFechamento: null,
  proximosStatus: [],
};

describe('acoesDeStatus', () => {
  it('cria um botão para cada status permitido pela API, na mesma ordem', () => {
    const acoes = acoesDeStatus({ ...chamadoBase, proximosStatus: ['EM_ATENDIMENTO', 'CANCELADO'] });

    expect(acoes.map((a) => a.rotulo)).toEqual(['Iniciar atendimento', 'Cancelar chamado']);
    expect(acoes[1].destrutiva).toBe(true);
  });

  it('bloqueia "Iniciar atendimento" enquanto não houver técnico', () => {
    const [semTecnico] = acoesDeStatus({ ...chamadoBase, proximosStatus: ['EM_ATENDIMENTO'] });
    const [comTecnico] = acoesDeStatus({ ...chamadoBase, tecnico: { id: 2, nome: 'Técnico' }, proximosStatus: ['EM_ATENDIMENTO'] });

    expect(semTecnico.bloqueio).toContain('Atribua um técnico');
    expect(comTecnico.bloqueio).toBeNull();
  });

  it('chama de "Reabrir" a volta ao atendimento de um chamado resolvido', () => {
    const acoes = acoesDeStatus({
      ...chamadoBase,
      status: 'RESOLVIDO',
      tecnico: { id: 2, nome: 'Técnico' },
      proximosStatus: ['FECHADO', 'EM_ATENDIMENTO'],
    });

    expect(acoes.map((a) => a.rotulo)).toEqual(['Confirmar e fechar', 'Reabrir chamado']);
  });

  it('não mostra ações quando a API não permite nenhuma (ex.: chamado fechado)', () => {
    expect(acoesDeStatus({ ...chamadoBase, status: 'FECHADO', proximosStatus: [] })).toEqual([]);
  });
});
