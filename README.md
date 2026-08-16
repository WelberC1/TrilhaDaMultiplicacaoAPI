# Trilha da Multiplicação API

Backend em ASP.NET Core (.NET 10) para o [Trilha da Multiplicação](https://github.com/WelberC1/TrilhaDaMultiplicacao.Desktop), substituindo o `SessionService` em memória do app desktop por persistência real de conta, progresso, ranking e conquistas.

## Estado atual — esboço básico e funcional

- **Autenticação**: registro e login por usuário com senha (BCrypt) + JWT (24h de validade) com refresh token (30 dias, rotacionado a cada uso) para renovação silenciosa sem pedir login de novo.
- **Segurança**: rate limiting (5 req/min em rotas de auth, 100 req/min global), bloqueio de conta após 5 logins errados (15 min), revogação de token via *security stamp* (logout e troca de senha invalidam tokens já emitidos na hora, e também revogam todo refresh token ativo do aluno), HSTS, CORS restritivo por padrão, headers de segurança básicos, segredos fora do repositório (`dotnet user-secrets`/variável de ambiente, com validação de startup que recusa subir sem uma chave JWT forte).
- **Recuperação de senha**: fluxo real por e-mail (código de 6 dígitos, expira em 15 min, bloqueia após 5 tentativas erradas). Envio via SMTP (MailKit) com fallback para log no console quando `Smtp:Host` não está configurado.
- **Troca de senha autenticada**: usuário já logado troca a senha sem precisar do fluxo de e-mail.
- **Logout de verdade**: invalida o token atual e todo refresh token ativo no servidor (não é só o cliente esquecer o token).
- **Perfil do aluno**: consultar e atualizar nome, e-mail e avatar.
- **Progresso**: registrar conclusão de fase (estrelas → pontos) e consultar progresso salvo; o servidor rejeita registrar uma fase fora de ordem (a fase anterior precisa estar concluída).
- **Ranking**: lista de alunos ordenada por pontos totais.
- **Conquistas**: catálogo fixo (seed), desbloqueadas automaticamente pelo número de fases concluídas.
- Persistência em **SQL Server** via EF Core, criado/migrado automaticamente ao iniciar.

## O que falta (próximos passos)

- Testes automatizados.

## Arquitetura

Controllers só traduzem HTTP ↔ chamada de service — nenhuma consulta EF Core aparece neles. Toda a lógica de negócio e acesso a dados vive em `Services/`, injetados via construtor (interfaces `IAuthService`, `IAlunoService`, `IProgressoService`, `IRankingService`, `IConquistaService`, `ITokenService`, `ICurrentUserService`).

Erros de negócio (e-mail duplicado, credenciais inválidas, aluno não encontrado) são `ApiException` lançadas pelos services e traduzidas em respostas HTTP por um `IExceptionHandler` central (`Middleware/ApiExceptionHandler.cs`), então os controllers não fazem `if/else` de status code.

Configuração do JWT é tipada (`Options/JwtOptions.cs`, bind de `appsettings.json:Jwt`) em vez de lida por indexador de string espalhado pelo código.

## Modelos

| Model | Descrição |
|---|---|
| `Aluno` | Conta do aluno: nome, e-mail, hash de senha, avatar. `PontosTotais` é calculado a partir do progresso. |
| `FaseProgresso` | Estrelas e pontos obtidos por `Aluno` em cada fase (`NumeroFase` único por aluno). |
| `Conquista` | Catálogo de conquistas (seed fixo), com critério simples de nº de fases concluídas. |
| `AlunoConquista` | Relação N:N entre aluno e conquista desbloqueada, com data. |

## Endpoints

Todos exceto `/api/auth/*` exigem `Authorization: Bearer <token>`.

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/registrar` | Cria conta e retorna token + refresh token + perfil. |
| POST | `/api/auth/login` | Autentica e retorna token + refresh token + perfil. |
| POST | `/api/auth/refresh` | Troca um refresh token válido por um novo par de tokens (rotação: o refresh token usado é revogado). |
| GET | `/api/alunos/me` | Perfil do aluno autenticado. |
| PUT | `/api/alunos/me` | Atualiza nome, e-mail e avatar. |
| GET | `/api/progresso` | Lista de fases concluídas pelo aluno. |
| POST | `/api/progresso/fases/{numero}` | Registra conclusão de uma fase (estrelas 0–3); só sobrescreve se a nova pontuação for maior. |
| GET | `/api/ranking` | Ranking geral por pontos, com o aluno atual sinalizado (`ehVoce`). |
| GET | `/api/conquistas` | Catálogo de conquistas com estado desbloqueada/bloqueada para o aluno atual. |
| POST | `/api/auth/esqueci-senha` | Envia um código de recuperação de 6 dígitos por e-mail (sempre 200, não revela se o e-mail existe). |
| POST | `/api/auth/redefinir-senha` | Confirma o código e define a nova senha. |
| POST | `/api/auth/logout` | Revoga o token atual (autenticado). |
| PUT | `/api/alunos/me/senha` | Troca a senha do aluno autenticado (pede a senha atual). |

Exemplos de requisição prontos em [`TrilhaDaMultiplicacaoAPI/TrilhaDaMultiplicacaoAPI.http`](TrilhaDaMultiplicacaoAPI/TrilhaDaMultiplicacaoAPI.http).

## Como rodar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) e uma instância de SQL Server acessível (local, LocalDB, Docker etc.). A connection string padrão em `appsettings.json` (`Server=localhost;Database=TrilhaDaMultiplicacao;Trusted_Connection=True;TrustServerCertificate=True`) usa autenticação do Windows contra uma instância local chamada `localhost` — ajuste conforme seu ambiente.

A API **recusa subir** sem uma chave JWT forte configurada (mínimo 32 bytes) — `appsettings.json` não tem mais nenhum segredo real. Configure localmente via `dotnet user-secrets` (uma vez só, por máquina de dev):

```bash
cd TrilhaDaMultiplicacaoAPI
dotnet user-secrets set "Jwt:Chave" "<uma-string-aleatoria-forte-aqui>"
```

Depois:

```bash
dotnet run --project TrilhaDaMultiplicacaoAPI --urls http://localhost:5271
```

O banco e as tabelas são criados/migrados automaticamente no primeiro start (`db.Database.Migrate()`). Em desenvolvimento, o OpenAPI fica disponível em `/openapi/v1.json`.

### Publicando em produção

Nunca reaproveite a chave de desenvolvimento. Configure via variável de ambiente no serviço de hospedagem (convenção do ASP.NET Core: `__` separa seção/chave):

- `Jwt__Chave` — string aleatória forte, só usada em produção.
- `Smtp__Host`, `Smtp__Usuario`, `Smtp__Senha`, `Smtp__RemetenteEmail` — credenciais SMTP reais (sem isso, a recuperação de senha só loga o código no console, ninguém recebe e-mail de verdade).
- `ConnectionStrings__Default` — string de conexão do SQL Server de produção.

A chave JWT que ficou commitada no histórico deste repositório (antes deste hardening) deve ser considerada **permanentemente comprometida** — nunca reutilize esse valor.

## Integração com o app desktop

O app desktop (`TrilhaDaMultiplicacao.Desktop`) já consome esta API de verdade para login, registro, logout, recuperação de senha, edição de perfil, progresso, ranking e conquistas (`Services/SessionService.cs` + `Services/ApiClient.cs`, base URL fixa em `http://localhost:5271`). Nenhuma tela do desktop usa dado mockado — a integração está completa.
