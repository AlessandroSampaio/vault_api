# VaultApi — Design

Data: 2026-08-29

## Contexto e objetivo

VaultApi é uma API .NET 10 responsável por gerenciar usuários, clientes, revendas, produtos e contratos (a relação comercial cliente/revenda). O contrato é a fonte da verdade do licenciamento de um cliente: quais produtos e módulos ele tem ativos, em que quantidade por tipo de unidade (servidor, estação, PDA, PDV), e a que preço — que pode divergir do preço de catálogo por negociação (fixo, desconto percentual ou valor).

O repositório foi inicializado com o template padrão do `dotnet new webapi` (WeatherForecast, etc). Todo esse conteúdo será descartado; só `VaultApi.slnx` permanece como arquivo raiz da solução, com os `<Project Path="..."/>` atualizados para a nova estrutura de projetos.

## Premissas de domínio (dadas pelo usuário)

- Usuário tem exatamente 1 Nivel: Admin, Revenda ou Usuario.
- Usuário pode ou não estar associado a uma Revenda. Sem revenda = usuário da matriz.
- Cliente pode ou não estar associado a uma Revenda. Sem revenda = cliente da matriz.
- Produto tem Módulos, que podem ser incluídos ou inativados.
- Produto tem valor de adesão/mensalidade no cadastro (catálogo).
- Contrato é a fonte da verdade do licenciamento — pode ter condições exclusivas: preço fixo, desconto valor ou percentual, aplicados por produto/módulo daquele contrato específico.
- Um mesmo produto, dentro de um contrato, pode ter múltiplas quantidades por tipo de unidade (ex.: 3 servidores/estação, 2 PDA, 5 PDV), cada tipo com preço próprio.
- Módulos podem ter variantes com cobrança adicional diferente por unidade (ex.: TEF tem tipos de integração distintos, cada um com custo adicional por PDV diferente).
- Contrato pode estar ativo/inativo.
- Só Admin gerencia contratos, revendas e usuários (CRUD/escrita).
- Revenda e Usuario podem ver produtos/módulos associados ao contrato do cliente, mas nunca os valores negociados.

## Decisões de modelagem (resolvidas no brainstorming)

1. **Precificação**: catálogo com preço base (Produto/Módulo/Variante) + override por item de contrato. Item sem override herda do catálogo; com override, o override vence.
2. **Tipo de unidade**: enum fixo no código (`Servidor`, `Estacao`, `PDA`, `PDV`). Não é tabela cadastrável — muda raro, simplicidade > flexibilidade de runtime aqui.
3. **Variantes de módulo**: entidade própria `ModuloVariante` (ex.: TEF-CliSiTef, TEF-Sitef), cada uma com valor adicional por unidade. Cobre o caso TEF hoje e casos futuros análogos sem precisar redesenhar Módulo.
4. **Autenticação**: ASP.NET Identity + JWT emitido pela própria API. Sem provedor externo no v1.
5. **Estrutura de projetos**: Clean Architecture com 4 projetos (Api, Application, Domain, Infrastructure) + Tests.
6. **Escopo de acesso Revenda/Usuario**: dupla restrição — (a) só enxergam clientes/contratos da própria RevendaId (nunca de outra revenda nem da matriz, a menos que o próprio usuário seja da matriz); (b) dentro do que enxergam, valores negociados (preço, desconto, override) nunca aparecem na resposta.
7. **Escopo Nivel=Usuario**: idêntico ao de Nivel=Revenda (mesma RevendaId, mesmo conjunto de clientes/contratos) — Usuario é tratado como "funcionário da revenda" com permissão de leitura, não como o cliente final.

## Arquitetura

Clean Architecture, 4 projetos + testes, sem CQRS/MediatR formal (domínio de produto ainda é admitidamente mutável pelo próprio autor do requisito — evitar boilerplate cedo):

- **Domain**: entidades puras, enums, interfaces de repositório (`IContratoRepository`, etc). Zero dependência de EF/ASP.NET.
- **Application**: casos de uso (services), DTOs, `PricingResolver` (regra pura de cálculo de preço), `IScopeFilter` (regra de escopo revenda/usuario), validação (FluentValidation).
- **Infrastructure**: EF Core + Npgsql, `AppDbContext`, migrations, implementação de repositórios, Identity stores, convenção snake_case para nomes de tabela/coluna.
- **Api**: controllers, emissão/validação de JWT, `Program.cs`, policies de autorização, mapeamento DTO↔domínio.
- **Tests**: xUnit + FluentAssertions, referencia os 4 projetos acima.

## Modelo de domínio

### Entidades

- **Usuario** (Identity, `IdentityUser<Guid>` customizado): Id, Nome, Email, Nivel (enum Admin/Revenda/Usuario), RevendaId (nullable).
- **Revenda**: Id, Nome, CNPJ, Ativo.
- **Cliente**: Id, Nome, CNPJ, RevendaId (nullable).
- **Produto**: Id, Nome, Descricao, ValorAdesaoBase, ValorMensalidadeBase, Ativo.
- **Modulo**: Id, ProdutoId, Nome, ValorAdesaoBase (nullable — null = incluso sem custo extra), ValorMensalidadeBase (nullable), Ativo/Inativo.
- **ModuloVariante**: Id, ModuloId, Nome, ValorAdicionalPorUnidade.
- **TipoUnidade** (enum): Servidor, Estacao, PDA, PDV.
- **Contrato**: Id, ClienteId, RevendaId (nullable, snapshot no momento da criação — não segue mudança futura de revenda do cliente), Ativo, DataInicio, DataFim (nullable).
- **ContratoItem**: Id, ContratoId, ProdutoId, ValorAdesaoOverride (nullable), ValorMensalidadeOverride (nullable), TipoDesconto (nullable, enum Fixo/Percentual), ValorDesconto (nullable).
- **ContratoItemUnidade**: Id, ContratoItemId, TipoUnidade, Quantidade. Tabela filha relacional (em vez de dicionário serializado) para permitir query direta por tipo de unidade.
- **ContratoItemModulo**: Id, ContratoItemId, ModuloId, ModuloVarianteId (nullable), Ativo, ValorOverride (nullable).

### Resolução de preço

`PricingResolver` (Application, puro, sem I/O) calcula o valor final de um `ContratoItem`:

1. Base = preço de catálogo do Produto + soma dos preços de Módulo/Variante ativos, multiplicado pela quantidade de cada `TipoUnidade` (via `ContratoItemUnidade`).
2. Se `ContratoItem` (ou `ContratoItemModulo`) tiver override: `Fixo` substitui o valor calculado; `Percentual`/`Valor` aplicam desconto sobre a base.
3. Resultado é sempre calculado em runtime a partir do catálogo + overrides — nunca persistido como valor final fixo, exceto no próprio override quando explícito.

## Autenticação e autorização

- Identity + JWT próprio. Claims no token: `sub`, `nivel`, `revenda_id` (ausente/null = matriz).
- Policies de rota:
  - `RequireAdmin`: única policy que permite escrita em Contrato, Revenda, Usuario, Produto/Módulo.
  - `RequireRevendaOrAdmin`: leitura para Revenda, Usuario e Admin.
- Escopo de dado aplicado no repositório (não no controller), via `IScopeFilter` injetado nos services:
  - Admin: sem filtro.
  - Revenda/Usuario: filtro `WHERE RevendaId == claim.revenda_id` (incluindo `null == null` para usuários de matriz) aplicado a toda query de Cliente/Contrato antes de qualquer outro filtro. Centralizar no repositório evita esquecer o filtro em endpoint novo.
- Ocultação de valores negociados: DTOs de resposta explicitamente diferentes por nível — `ContratoAdminDto` (com preços/overrides) vs `ContratoPublicoDto` (sem campos de preço). Controller decide qual DTO retornar com base no claim `nivel`, não por máscara condicional dentro do mesmo objeto.

## Infraestrutura

- PostgreSQL via Npgsql + EF Core 10. Convenção snake_case para tabelas/colunas.
- Migrations em `Infrastructure/Migrations`, geradas via `dotnet ef migrations add`. Aplicação automática apenas em Development; produção aplica via script/CI dedicado (não auto-apply).
- `docker-compose.yml` na raiz com Postgres para desenvolvimento local.

## Testes

- xUnit + FluentAssertions.
- `PricingResolver` e `IScopeFilter`: testes puros, sem banco.
- Testes de integração de endpoint: Testcontainers com Postgres real (evita divergência entre provider in-memory do EF e comportamento real do Postgres).

## Documentação

- `README.md` na raiz: visão geral do domínio (premissas acima), stack, como rodar localmente (docker-compose), como rodar migrations, como rodar testes, estrutura de pastas, convenções de Nivel/RevendaId/escopo.

## Fora de escopo (v1)

- Tabela de preço versionada por vigência/data (reajuste anual) — mencionada como alternativa mas não escolhida; pode ser revisitada depois se necessário.
- TipoUnidade cadastrável via banco — fica fixo em enum até haver necessidade real de extensão em runtime.
- Provedor de identidade externo (Auth0/Keycloak/Azure AD B2C).
- CQRS/MediatR — services simples por ora.
