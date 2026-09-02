# VaultApi

API .NET 10 para gestao de usuarios, clientes, revendas, produtos e contratos —
a relacao comercial cliente/revenda que licencia o uso dos produtos.

## Dominio

- Usuario tem 1 Nivel: `Admin`, `Revenda` ou `Usuario`. Usuario/Cliente sem
  `RevendaId` pertence a matriz.
- Produto tem preco por tipo de unidade (`Servidor`, `Estacao`, `PDA`, `PDV`)
  via `ProdutoPrecoUnidade`, e Modulos (com Variantes que cobram adicional por
  unidade, ex.: TEF por PDV).
- Contrato e a fonte da verdade do licenciamento: define quantidade por tipo
  de unidade e modulos ativos por produto, com overrides de preco (fixo,
  percentual ou valor) por item.
- So Admin escreve Contrato/Revenda/Usuario/Produto. Revenda e Usuario tem
  leitura restrita a propria `RevendaId` e nunca veem valores negociados.
- Cada item de contrato gera uma Licenca (serial opaco versionado); o
  algoritmo de criptografia real ainda sera definido.

Detalhes completos: `docs/superpowers/specs/2026-08-29-vault-api-design.md`.

## Stack

.NET 10, ASP.NET Core Identity, EF Core + Npgsql, PostgreSQL, JWT Bearer,
xUnit + FluentAssertions + Testcontainers.

## Rodando localmente

```bash
docker compose up -d
dotnet ef database update --project src/VaultApi.Infrastructure --startup-project src/VaultApi.Api
dotnet run --project src/VaultApi.Api
```

## Rodando os testes

Requer Docker (Testcontainers sobe um Postgres real por execucao):

```bash
dotnet test
```

## Estrutura

```
src/VaultApi.Domain          entidades, enums, interfaces de repositorio
src/VaultApi.Application     services, DTOs, PricingResolver, IScopeFilter
src/VaultApi.Infrastructure  EF Core, Identity, repositorios, migrations
src/VaultApi.Api             controllers, JWT, policies de autorizacao
tests/VaultApi.Tests         testes unitarios e de integracao
```

## Convencoes

- `nivel` e `revenda_id` sao claims do JWT; `RequireAdmin` e
  `RequireRevendaOrAdmin` sao as policies de autorizacao.
- Escopo de dado (Revenda/Usuario so veem a propria `RevendaId`) e aplicado
  no repositorio via `IScopeFilter`, nao no controller — nunca filtre so na
  camada de apresentacao.
- Migrations aplicam automaticamente so em Development; producao aplica via
  `dotnet ef database update` manual/CI.
