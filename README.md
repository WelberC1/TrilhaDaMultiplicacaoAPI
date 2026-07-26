# Trilha da Multiplicação API

Backend em ASP.NET Core (.NET 10) para o [Trilha da Multiplicação](https://github.com/WelberC1/TrilhaDaMultiplicacao.Desktop), substituindo o `SessionService` em memória do app desktop por persistência real de conta, progresso, ranking e conquistas.

## Estado atual — esboço básico e funcional

- **Autenticação**: registro e login com senha (BCrypt) + JWT (7 dias de validade).
- **Recuperação de senha**: fluxo real por e-mail (código de 6 dígitos, expira em 15 min, bloqueia após 5 tentativas erradas). Envio via SMTP (MailKit) com fallback para log no console quando `Smtp:Host` não está configurado.
- **Perfil do aluno**: consultar e atualizar nome, e-mail e avatar.
- **Progresso**: registrar conclusão de fase (estrelas → pontos) e consultar progresso salvo.
- **Ranking**: lista de alunos ordenada por pontos totais.
- **Conquistas**: catálogo fixo (seed), desbloqueadas automaticamente pelo número de fases concluídas.
- Persistência em **SQL Server** via EF Core, criado/migrado automaticamente ao iniciar.

## O que falta (próximos passos)

- Troca de senha autenticada (usuário já logado, sem passar pelo fluxo de "esqueci minha senha").
- Refresh token / revogação de token.
- Validação mais rica de fases (ex.: impedir pular fases fora de ordem, se isso vier a importar no servidor).
- Testes automatizados.
- Trocar a chave JWT e as credenciais SMTP em `appsettings.json` por segredos de produção (via variável de ambiente ou `dotnet user-secrets`) antes de qualquer deploy.

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
| POST | `/api/auth/registrar` | Cria conta e retorna token + perfil. |
| POST | `/api/auth/login` | Autentica e retorna token + perfil. |
| GET | `/api/alunos/me` | Perfil do aluno autenticado. |
| PUT | `/api/alunos/me` | Atualiza nome, e-mail e avatar. |
| GET | `/api/progresso` | Lista de fases concluídas pelo aluno. |
| POST | `/api/progresso/fases/{numero}` | Registra conclusão de uma fase (estrelas 0–3); só sobrescreve se a nova pontuação for maior. |
| GET | `/api/ranking` | Ranking geral por pontos, com o aluno atual sinalizado (`ehVoce`). |
| GET | `/api/conquistas` | Catálogo de conquistas com estado desbloqueada/bloqueada para o aluno atual. |
| POST | `/api/auth/esqueci-senha` | Envia um código de recuperação de 6 dígitos por e-mail (sempre 200, não revela se o e-mail existe). |
| POST | `/api/auth/redefinir-senha` | Confirma o código e define a nova senha. |

Exemplos de requisição prontos em [`TrilhaDaMultiplicacaoAPI/TrilhaDaMultiplicacaoAPI.http`](TrilhaDaMultiplicacaoAPI/TrilhaDaMultiplicacaoAPI.http).

## Como rodar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download) e uma instância de SQL Server acessível (local, LocalDB, Docker etc.). A connection string padrão em `appsettings.json` (`Server=localhost;Database=TrilhaDaMultiplicacao;Trusted_Connection=True;TrustServerCertificate=True`) usa autenticação do Windows contra uma instância local chamada `localhost` — ajuste conforme seu ambiente.

```bash
dotnet run --project TrilhaDaMultiplicacaoAPI --urls http://localhost:5271
```

O banco e as tabelas são criados/migrados automaticamente no primeiro start (`db.Database.Migrate()`). Em desenvolvimento, o OpenAPI fica disponível em `/openapi/v1.json`.

## Integração com o app desktop

O app desktop (`TrilhaDaMultiplicacao.Desktop`) já consome esta API de verdade para login, registro, logout, recuperação de senha e edição de perfil (`Services/SessionService.cs` + `Services/ApiClient.cs`, base URL fixa em `http://localhost:5271`). Trilha/progresso/ranking/conquistas ainda são mockados no desktop, aguardando a próxima fase de integração.
