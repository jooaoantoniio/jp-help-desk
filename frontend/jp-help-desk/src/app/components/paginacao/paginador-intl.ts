import { Injectable } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

/**
 * Textos do paginador do Angular Material em português.
 * Uso: providers: [{ provide: MatPaginatorIntl, useClass: PaginadorIntl }] no componente da tela.
 */
@Injectable()
export class PaginadorIntl extends MatPaginatorIntl {
  override itemsPerPageLabel = 'Itens por página';
  override nextPageLabel = 'Próxima página';
  override previousPageLabel = 'Página anterior';
  override firstPageLabel = 'Primeira página';
  override lastPageLabel = 'Última página';

  override getRangeLabel = (pagina: number, tamanho: number, total: number): string => {
    if (total === 0) {
      return '0 de 0';
    }
    const inicio = pagina * tamanho + 1;
    const fim = Math.min(inicio + tamanho - 1, total);
    return `${inicio} – ${fim} de ${total}`;
  };
}
