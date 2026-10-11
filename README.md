# Product Compass

Foundation for a collaborative product-discovery SaaS. This milestone provides the application shell, basic navigation, a protected organization workspace read, and PostgreSQL persistence for organizations, memberships, and workspaces. Customer signals, needs, initiatives, and AI analysis are intentionally **not implemented yet**. The UI labels these workflows as planned and shows no invented product metrics or customer records.

## Prerequisites

- Node.js 24 and npm
- .NET 10 SDK (selected by `global.json`)
- Docker with Compose, or an existing PostgreSQL 17 instance

Run commands from the repository root unless stated otherwise.

## 1. Start PostgreSQL

```sh
cp .env.example .env
# Edit .env and set a local POSTGRES_PASSWORD.
docker compose up -d --wait
```

PostgreSQL is bound to `127.0.0.1:5432`. Its data lives in the named `postgres_data` volume and survives `docker compose down`. Do not use `down -v` unless you intend to delete local data. An existing PostgreSQL instance also works; create the database and user yourself and use its connection string below.

## 2. Configure and initialize the API

```sh
dotnet restore
dotnet tool restore
dotnet user-secrets set 'ConnectionStrings:Database' 'Host=localhost;Port=5432;Database=product_compass;Username=product_compass;Password=YOUR_LOCAL_PASSWORD' --project src/backend/ProductCompass.Api

dotnet ef database update --project src/backend/ProductCompass.Infrastructure --startup-project src/backend/ProductCompass.Api -- --environment Development

dotnet run --project src/backend/ProductCompass.Api -- --seed-development
```

Replace `YOUR_LOCAL_PASSWORD` with the password from `.env`. Compose reads `.env`; the API deliberately reads user-secrets or environment variables instead. Never commit credentials. The equivalent deployment variable is `ConnectionStrings__Database`.

The explicit development seed command applies outstanding migrations and creates a sample organization, workspace, and membership if absent. It exits after seeding and is safe to repeat on an intact local database. It refuses to run outside Development. Normal API startup neither migrates the database nor creates memberships.

## 3. Create a local access token and start the API

```sh
dotnet user-jwts create --project src/backend/ProductCompass.Api --name developer --audience http://localhost:5200
dotnet run --project src/backend/ProductCompass.Api
```

Keep the printed token locally. Its signing key is held in .NET user-secrets, not the repository. The development token subject `developer` and issuer `dotnet-user-jwts` match the seeded membership. Run the token command again if the token expires. The API listens at `http://localhost:5200` via the checked-in launch profile.

- `GET /health/live`: public process health.
- `GET /health/ready`: public database connectivity and pending-migration check; returns 503 until ready, without database details.
- `GET /api/organizations/{organizationId}/workspace`: requires a valid bearer token and matching organization membership. Invalid credentials return 401; absent membership or an unknown organization returns 403; malformed organization IDs return 400.

## 4. Start the frontend

In a second terminal:

```sh
cd src/frontend
npm ci
npm run dev
```

Open **http://127.0.0.1:5173**. The Vite proxy forwards `/api` and `/health` to the local backend, avoiding permissive CORS configuration.

The overview and navigation work immediately as a foundation preview. To check persistence and authorization end-to-end, select **Connect** in the overview, use organization ID `11111111-1111-1111-1111-111111111111`, and paste the development token. A successful request displays the persisted organization and workspace names. The token is held only in component memory during the connection flow and is not stored in localStorage, a cookie, a URL, or a configuration file. Reloading returns to the preview. This is a developer connectivity check, not the future production sign-in flow; the token input is excluded from production builds.

## Build and test

```sh
dotnet build ProductCompass.sln --configuration Release
dotnet test ProductCompass.sln --configuration Release
cd src/frontend
npm run build
npm test
```

The backend tests cover domain validation, rejection before persistence, relational membership isolation (including issuer collisions), migration/model alignment, real JWT validation at the HTTP boundary, missing membership, malformed IDs, and sanitized error responses. Relational unit tests use isolated in-memory SQLite; production persistence uses Npgsql/PostgreSQL. Frontend tests check authorized request headers and meaningful API errors. GitHub Actions runs both builds and test suites.

`npm run preview` serves the frontend production bundle but does not proxy the backend; its purpose is static UI inspection. A deployed frontend needs an HTTPS reverse proxy for `/api`, plus SPA fallback to `index.html` for direct navigation.

## Repository and architecture

```text
src/
  frontend/                         React + TypeScript + Vite
  backend/
    ProductCompass.Domain/          Workspace entities and invariants
    ProductCompass.Application/     Authorized use cases, repository and AI contracts
    ProductCompass.Infrastructure/  EF Core, Npgsql, migrations, scoped queries
    ProductCompass.Api/             HTTP, JWT authentication, composition root
tests/ProductCompass.Tests/        Domain, persistence and HTTP tests
compose.yaml                      Local PostgreSQL with persistent volume
```

This is a modular monolith with one backend deployable. Workspaces form the first vertical module inside each layer. Future Signals, Needs, Initiatives and Analysis modules should follow the same namespace/folder pattern, with application contracts at module boundaries. Domain and Application do not depend on ASP.NET, EF Core, or React. Keep business rules out of endpoint handlers and UI components.

One organization currently has one workspace. Membership keys combine organization, token subject, and token issuer. The repository has no unscoped workspace read: its single SQL query joins the organization and checks membership in the requested organization. The route ID only selects the desired organization; it does not confer authority. There is no general data-listing endpoint or client-selected identity header. Future organization-owned records need explicit organization predicates, authorization checks, and composite constraints for links to prevent cross-organization associations.

The provider-independent `INeedAnalysisProvider` contract lives in Application. There is no active provider, AI endpoint, or frontend API key. When implemented, resolve provider credentials on the server, load signals through an authorized organization scope, and preserve a clear distinction between generated proposals and human-verified needs.

## Migrations

The initial checked-in EF Core migration creates the `workspaces` schema, organization/workspace foreign keys, issuer-aware memberships, and a unique workspace-per-organization index. For subsequent changes:

```sh
dotnet ef migrations add DescriptiveName --project src/backend/ProductCompass.Infrastructure --startup-project src/backend/ProductCompass.Api --output-dir Persistence/Migrations -- --environment Development
dotnet ef database update --project src/backend/ProductCompass.Infrastructure --startup-project src/backend/ProductCompass.Api -- --environment Development
```

Apply reviewed migrations as a deployment step with a separate privileged database identity; do not grant runtime schema-management privileges in production.

## Before production

This is a local development foundation, not a production-ready commercial release. Outside Development, JWT validation requires `Authentication__Authority` (an HTTPS OIDC issuer) and `Authentication__Audience`. Configure `AllowedHosts`, HTTPS termination, an appropriately restricted database account, and a secret manager. Never use `dotnet user-jwts` or seeded development memberships in production. Provision memberships using the exact verified production issuer and subject. Production user sign-in, invitations, organization provisioning, roles, audit history, rate limiting, deployment hardening, backup/restore and operations remain future work.

## Next milestones

1. Production identity integration and organization onboarding; tenant-isolation integration tests against PostgreSQL in CI.
2. Customer Signals: create, edit, list, validation and durable organization-scoped storage.
3. Customer Needs: create, edit, link multiple signals, and preserve evidence and review status.
4. AI analysis: server-side provider adapter, explicit reviewable proposals, source attribution and human approval.
5. Initiatives linked to needs, followed by collaborative workflows and product knowledge history.

The foundation deliberately avoids adding empty feature database tables or CRUD endpoints before those workflows are designed.

## Initial verification

Verified locally on 11 October 2026 with .NET SDK 10.0.401 and Node 24.21.0:

- Backend Release build and all 17 tests passed.
- Frontend production build and all 5 tests passed.
- Migration applied to an isolated real PostgreSQL instance; development seed ran twice successfully.
- Live API returned the expected 200, 400, 401 and 403 responses; readiness was 200.
- Headless Chrome passed all navigation routes, authorized workspace connection, desktop and 390px mobile layout checks, with no JavaScript errors or persistent token storage.
- npm and NuGet dependency audits reported no known vulnerabilities at verification time.

Docker was not installed on the verification machine, so the Compose startup itself was not executed. Database smoke checks used temporary PostgreSQL 18.4 binaries; the Compose configuration targets PostgreSQL 17. The temporary verification tools are outside the repository and are not application dependencies. Temporary services were stopped after verification; use the setup commands above to start your own local instance.
