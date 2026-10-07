// Configuração de produção. A API é servida na mesma origem do frontend, sob /api
// (ex.: via proxy reverso), por isso a URL é relativa.
export const environment = {
  production: true,
  apiUrl: '/api',
};
