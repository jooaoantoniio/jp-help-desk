import { Routes } from '@angular/router';
import { authGuard, perfilGuard, visitanteGuard } from './guards/auth.guards';

// Cada página é carregada sob demanda (lazy loading): o código só é baixado ao acessar a rota.
// As rotas dentro do Shell exigem login e recebem sidebar + header; o login fica fora dele.
export const routes: Routes = [
  {
    path: 'login',
    title: 'Entrar | JP Help Desk',
    canActivate: [visitanteGuard],
    loadComponent: () => import('./pages/login/login').then((m) => m.Login),
  },
  {
    path: '',
    canActivate: [authGuard],
    // O layout também é lazy: a tela de login não precisa baixar sidebar, menu etc.
    loadComponent: () => import('./components/layout/shell/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Dashboard | JP Help Desk',
        loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'chamados',
        title: 'Chamados | JP Help Desk',
        loadComponent: () => import('./pages/chamados-lista/chamados-lista').then((m) => m.ChamadosLista),
      },
      {
        path: 'chamados/novo',
        title: 'Novo chamado | JP Help Desk',
        loadComponent: () => import('./pages/chamado-form/chamado-form').then((m) => m.ChamadoForm),
      },
      {
        path: 'chamados/:id',
        title: 'Chamado | JP Help Desk',
        loadComponent: () => import('./pages/chamado-detalhe/chamado-detalhe').then((m) => m.ChamadoDetalhe),
      },
      {
        path: 'chamados/:id/editar',
        title: 'Editar chamado | JP Help Desk',
        loadComponent: () => import('./pages/chamado-form/chamado-form').then((m) => m.ChamadoForm),
      },
      {
        path: 'usuarios',
        title: 'Usuários | JP Help Desk',
        canActivate: [perfilGuard('ADMIN')],
        loadComponent: () => import('./pages/usuarios/usuarios').then((m) => m.Usuarios),
      },
      {
        path: 'categorias',
        title: 'Categorias | JP Help Desk',
        canActivate: [perfilGuard('ADMIN')],
        loadComponent: () => import('./pages/categorias/categorias').then((m) => m.Categorias),
      },
      {
        path: 'perfil',
        title: 'Meu perfil | JP Help Desk',
        loadComponent: () => import('./pages/perfil/perfil').then((m) => m.Perfil),
      },
      {
        path: '**',
        title: 'Página não encontrada | JP Help Desk',
        loadComponent: () => import('./pages/nao-encontrada/nao-encontrada').then((m) => m.NaoEncontrada),
      },
    ],
  },
];
