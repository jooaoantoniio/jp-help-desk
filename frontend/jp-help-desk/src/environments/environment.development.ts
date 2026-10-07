// Configuração de desenvolvimento (ng serve). As chamadas para /api são encaminhadas
// para a API .NET pelo proxy definido em proxy.conf.json — sem problemas de CORS.
export const environment = {
  production: false,
  apiUrl: '/api',
};
