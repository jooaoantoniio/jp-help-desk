# JP Help Desk — Frontend

Aplicação Angular 22 do JP Help Desk. Visão geral do projeto, arquitetura e instruções completas no
[README principal](../../README.md).

## Comandos

```bash
npm install
npm start                      # http://localhost:4200 (proxy de /api para a API em :5288 — ver proxy.conf.json)
npx ng test --watch=false      # testes unitários (Vitest)
npx ng build                   # build de produção em dist/jp-help-desk/browser
```

A API precisa estar rodando em `http://localhost:5288` (`dotnet run --project backend/JpHelpDesk.Api`).

## Estrutura

```
src/app/
├── components/     # layout (shell, sidebar, header), badges, diálogos, gráfico, força da senha
├── pages/          # uma pasta por tela (carregadas sob demanda)
├── services/       # acesso à API e tratamento de erros
├── guards/         # autenticação e perfil
├── interceptors/   # token JWT e sessão expirada
├── models/         # tipos do contrato da API
└── validacao/      # regras de senha (espelho da API)
```
