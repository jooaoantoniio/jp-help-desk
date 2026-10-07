import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** Componente raiz: apenas o ponto onde o roteador renderiza a página atual. */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class App {}
