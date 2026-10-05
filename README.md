# Desafio Dev

```
data/                    vendas.json e estoque.json do enunciado (carga inicial)
src/DesafioDev.Core      regras de negócio, sem dependência de IO
src/DesafioDev.Infra     EF Core + SQLite (estoque persistido, migrations)
src/DesafioDev.Api       ASP.NET Core Minimal API em http://localhost:5080
src/DesafioDev.App       console original (estoque em memória)
tests/DesafioDev.Tests   xUnit
web/                     Angular 22 + Angular Material
```

## Com Docker

Só precisa do Docker (no Windows, Docker Desktop com WSL 2):

```bash
docker compose up --build
```

Abra http://localhost:8080. São dois contêineres: `web` (nginx servindo o Angular e repassando `/api`)
e `api` (ASP.NET Core). O banco fica no volume `estoque-dados` e sobrevive a `docker compose down`;
para zerar o estoque, use `docker compose down -v`.

Variáveis opcionais (num arquivo `.env` ao lado do compose ou no ambiente):

| Variável | Padrão | |
|---|---|---|
| `WEB_PORT` | 8080 | porta do host para o front |
| `TZ` | America/Campo_Grande | fuso usado como "hoje" no cálculo de juros |

## Sem Docker

### Pré-requisitos

- .NET 8 SDK
- Node.js 22.22.3+ ou 24.15+ (exigência do Angular CLI 22)

### Rodando

Terminal 1, API (cria `estoque.db` e aplica a migration na primeira execução):

```bash
dotnet run --project src/DesafioDev.Api
```

Terminal 2, front:

```bash
cd web
npm install
npm start
```

Abra http://localhost:4200. O `ng serve` encaminha `/api` para a porta 5080 (`web/proxy.conf.json`).

Para zerar o estoque, pare a API e apague `src/DesafioDev.Api/estoque.db*`: os produtos são recarregados do `data/estoque.json`.

## Testes

```bash
dotnet test
cd web && npm test -- --watch=false
```

## Endpoints

| Método | Rota | |
|---|---|---|
| GET | /api/comissoes | comissão por vendedor, com o detalhe de cada venda |
| GET | /api/estoque/produtos | saldos |
| GET | /api/estoque/movimentacoes?codigoProduto= | histórico (mais recente primeiro) |
| GET | /api/estoque/movimentacoes/{id} | uma movimentação |
| POST | /api/estoque/movimentacoes | `{ codigoProduto, tipo: "Entrada" ou "Saida", quantidade, descricao }`, devolve 201 com saldo anterior e final |
| GET | /api/juros?valor=1000.00&vencimento=2026-09-25 | juros na data de hoje |

Erros seguem ProblemDetails: 400 (validação, com `errors` por campo), 404 (produto), 422 (saldo insuficiente), 409 (conflito de concorrência).
`src/DesafioDev.Api/DesafioDev.Api.http` tem exemplos prontos para o REST client do VS, Rider ou VS Code.

## Migrations

A migration inicial está em `src/DesafioDev.Infra/Migrations`. Para alterar o modelo:

```bash
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add NomeDaMudanca -p src/DesafioDev.Infra -s src/DesafioDev.Api
```
