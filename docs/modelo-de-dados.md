# Modelo de dados — JP Help Desk

## Diagrama entidade-relacionamento

```mermaid
erDiagram
    USUARIO ||--o{ CHAMADO : "abre (solicitante)"
    USUARIO |o--o{ CHAMADO : "atende (técnico)"
    CATEGORIA ||--o{ CHAMADO : classifica
    CHAMADO ||--|{ HISTORICO_CHAMADO : possui
    USUARIO ||--o{ HISTORICO_CHAMADO : registra

    USUARIO {
        int Id PK
        string Nome "100"
        string Email UK "150"
        string SenhaHash
        string Perfil "ADMIN | TECNICO | USUARIO"
        bool Ativo
        datetimeoffset CriadoEm
        datetimeoffset AtualizadoEm "nullable"
    }

    CATEGORIA {
        int Id PK
        string Nome UK "50"
        string Descricao "250, nullable"
        bool Ativo
        datetimeoffset CriadoEm
    }

    CHAMADO {
        int Id PK
        string Titulo "150"
        string Descricao "4000"
        string Status
        string Prioridade
        int CategoriaId FK
        int SolicitanteId FK
        int TecnicoId FK "nullable"
        datetimeoffset DataAbertura
        datetimeoffset DataAtualizacao "nullable"
        datetimeoffset DataFechamento "nullable"
    }

    HISTORICO_CHAMADO {
        int Id PK
        int ChamadoId FK
        int UsuarioId FK
        string Tipo
        string Descricao "1000"
        datetimeoffset DataRegistro
    }
```

## Enumerações

| Enum | Valores |
|---|---|
| `PerfilUsuario` | `ADMIN`, `TECNICO`, `USUARIO` |
| `StatusChamado` | `ABERTO`, `EM_ATENDIMENTO`, `AGUARDANDO_USUARIO`, `RESOLVIDO`, `FECHADO`, `CANCELADO` |
| `PrioridadeChamado` | `BAIXA`, `MEDIA`, `ALTA`, `CRITICA` |
| `TipoHistorico` | `ABERTURA`, `ATRIBUICAO`, `ALTERACAO_STATUS`, `ALTERACAO_DADOS`, `COMENTARIO` |

## Decisões de modelagem

- **Enums gravados como texto** (`varchar`) e não como número: o banco fica legível
  (`WHERE Status = 'ABERTO'`) e reordenar o enum no código não corrompe dados.
- **`DateTimeOffset` em UTC**: datas guardam o fuso, evitando ambiguidade entre servidor e cliente.
  A conversão para o horário local é responsabilidade do frontend.
- **Exclusão restrita (`Restrict`)** nas chaves para `Usuario` e `Categoria`: não é possível
  apagar um usuário ou categoria com chamados vinculados — eles são **desativados** (`Ativo = false`).
- **Histórico em cascata**: o histórico pertence ao chamado; removido o chamado, remove-se o histórico.
- **Comentários e observações** de técnicos e usuários são registros de histórico do tipo `COMENTARIO`,
  mantendo uma linha do tempo única por chamado.
- **Índices** em `Chamado.Status`, `Chamado.Prioridade` e `Chamado.DataAbertura` para filtros e dashboard;
  índices únicos em `Usuario.Email` e `Categoria.Nome`.
