# Muonroi.Modular.Template

A .NET solution template for building Modular Monolith applications using ASP.NET Core with the Muonroi.BuildingBlock library. Perfect for enterprise systems with medium complexity that need module separation while keeping deployment simple.

## Quick Start

```bash
# 1. Install template
dotnet new install Muonroi.Modular.Template

# 2. Create new project
dotnet new mr-mod-sln -n MyModularApp

# 3. Setup
cd MyModularApp/MyModularApp
dotnet restore

# 4. Run migrations
cd src/Host/MyModularApp.Host
dotnet ef migrations add InitialCreate --project ../../Shared/MyModularApp.Kernel
dotnet ef database update --project ../../Shared/MyModularApp.Kernel

# 5. Run
dotnet run
```

Open: `https://localhost:5001/swagger`

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- (Optional) [EF Core CLI](https://docs.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

## Installation

### From NuGet (recommended)

```bash
dotnet new install Muonroi.Modular.Template
```

### From source

```bash
git clone <your-private-url>/Muonroi.Modular.Template.git
cd Muonroi.Modular.Template
dotnet new install ./
```

### Verify installation

```bash
dotnet new list | grep "mr-mod-sln"
```

## Usage

### Create new project

```bash
dotnet new mr-mod-sln -n <ProjectName> [--ui <angular|react|mvc|none>]
```

| Parameter | Short | Description | Default |
|-----------|-------|-------------|---------|
| `--name` | `-n` | Solution/project name | (required) |
| `--UiFramework` | `--ui` | Frontend shell scaffold (`angular`, `react`, `mvc`, `none`) | `none` |

Generated UI shells use Muonroi hybrid UI engine:

- Backend metadata endpoints: `GET /api/v1/auth/ui-engine/{userId}`, `GET /api/v1/auth/ui-engine/current`
- FE runtime packages: `@muonroi/ui-engine-core`, `@muonroi/ui-engine-angular`, `@muonroi/ui-engine-react`
- MVC runtime package: `Muonroi.Ui.Engine.Mvc`

### Examples

```bash
# Create modular monolith for e-commerce
dotnet new mr-mod-sln -n ECommerce

# Creates modules: Identity, Catalog with shared Kernel

# Create modular monolith with React shell starter
dotnet new mr-mod-sln -n ECommerce --ui react
```

## Project Structure

```
MyModularApp/
├── MyModularApp.sln
├── src/
│   ├── Host/                              # Application Entry Point
│   │   └── MyModularApp.Host/             # Web API Host
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── appsettings.Production.json
│   │       ├── Program.cs                  # Bootstrapper
│   │       └── Infrastructures/            # DI Configuration
│   ├── Modules/                            # Independent Business Modules
│   │   ├── Identity/                       # User/Auth module
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   └── IdentityModule.cs          # Module registration
│   │   └── Catalog/                        # Product catalog module
│   │       ├── Controllers/
│   │       ├── Models/
│   │       ├── Services/
│   │       └── CatalogModule.cs           # Module registration
│   └── Shared/                             # Shared Infrastructure
│       ├── MyModularApp.Kernel/            # Data layer, DbContext
│       │   ├── Persistence/
│       │   │   ├── MyModularAppDbContext.cs
│       │   │   └── Migrations/
│       │   └── Repositories/
│       └── MyModularApp.Shared/            # Shared contracts
│           ├── Events/                     # Domain events
│           └── Dtos/                       # Shared DTOs
└── README.md
```

## Configuration

### Supported Database Types

| DbType | Connection String Key |
|--------|----------------------|
| `Sqlite` | `SqliteConnectionString` |
| `SqlServer` | `SqlServerConnectionString` |
| `MySql` | `MySqlConnectionString` |
| `PostgreSql` | `PostgreSqlConnectionString` |
| `MongoDb` | `MongoDbConnectionString` |

### Example Configuration

```json
{
  "DatabaseConfigs": {
    "DbType": "Sqlite",
    "ConnectionStrings": {
      "SqliteConnectionString": "Data Source=modular_app.db"
    }
  },
  "TokenConfigs": {
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:5001",
    "SymmetricSecretKey": "your-secret-key-minimum-32-characters!",
    "UseRsa": false,
    "ExpiryMinutes": 60
  },
  "EnableEncryption": false
}
```

### Feature Flags

Toggle optional features to reduce startup time:

```json
{
  "FeatureFlags": {
    "UseGrpc": false,
    "UseServiceDiscovery": false,
    "UseMessageBus": false,
    "UseBackgroundJobs": false,
    "UseEnsureCreatedFallback": true
  }
}
```

### Enterprise Operations (E4/E5)

Enable management endpoints only when needed:

```json
{
  "EnterpriseOps": {
    "EnableComplianceEndpoints": false,
    "EnableOperationsEndpoints": false
  }
}
```

When enabled:
- `EnableComplianceEndpoints`: maps `MapMComplianceEndpoints()`
- `EnableOperationsEndpoints`: maps `MapMEnterpriseOperationsEndpoints()`

Ops scripts included in `scripts/`:
- `check-enterprise-upgrade-compat.ps1`
- `check-enterprise-slo-gates.ps1`

SLO presets included in `deploy/enterprise/slo-presets/`:
- `balanced.json`
- `strict.json`
- `regulated.json`

Note: if your `Muonroi.BuildingBlock` package version does not include E4/E5 endpoint extensions yet, these toggles are ignored safely.

## Database Migrations

Since modules share a single database (via Kernel), migrations are centralized:

```bash
# Add migration
dotnet ef migrations add "AddNewFeature" \
    -p ./src/Shared/MyModularApp.Kernel \
    --startup-project ./src/Host/MyModularApp.Host \
    -o Persistence/Migrations

# Update database
dotnet ef database update \
    -p ./src/Shared/MyModularApp.Kernel \
    --startup-project ./src/Host/MyModularApp.Host
```

## Modular Monolith Architecture

### Why Modular Monolith?

- **Simpler than Microservices** - Single deployment, no distributed complexity
- **Better than Monolith** - Clear module boundaries, independent development
- **Migration Path** - Easy to extract modules into microservices later

### Module Communication

Modules communicate via:
1. **Domain Events** - In-process async communication
2. **Integration Events** - Cross-module events via MediatR
3. **Shared Contracts** - DTOs in `Shared` project (use sparingly)
4. **Direct References** - Avoid when possible

### Adding New Module

1. Create module folder under `src/Modules/`:
   ```bash
   src/Modules/
   └── Orders/                    # New module
       ├── Controllers/
       ├── Models/
       ├── Services/
       └── OrdersModule.cs       # Module registration
   ```

2. Add module project reference to Host

3. Register module services in `Program.cs`:
   ```csharp
   builder.Services.AddOrdersModule();
   ```

4. Add module entities to Kernel DbContext (optional)

## Features

- **Modular Monolith** - Independent modules, single deployment
- **CQRS with MediatR** - Command/Query separation per module
- **Authentication & Authorization** - JWT, permissions, roles
- **Entity Framework Core** - Shared Kernel database
- **Structured Logging** - Serilog with service-specific log files
- **Caching** - Memory, Redis, or Multi-level
- **Multi-tenancy** - Data isolation by tenant
- **Inter-Module Events** - Loose coupling via domain events
- **Service Discovery** - Consul integration
- **Message Bus** - Kafka/RabbitMQ via MassTransit
- **Background Jobs** - Hangfire/Quartz integration

## Module Guidelines

✅ **DO:**
- Keep modules independent and loosely coupled
- Use events for inter-module communication
- Share only necessary contracts (DTOs/interfaces)
- Follow Domain-Driven Design within modules
- Use separate folders for module concerns

❌ **DON'T:**
- Create direct module-to-module dependencies
- Share domain entities across modules
- Put business logic in Shared projects
- Mix module responsibilities

## Documentation

Private docs are centralized in `Muonroi.Docs`:

- [License Capability Model](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/license-capability-model.md)
- [Control Plane MVP](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/control-plane-mvp.md)
- [Enterprise Secure Profile (E2)](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/enterprise-secure-profile-e2.md)
- [Enterprise Centralized Authorization (E3)](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/enterprise-centralized-authorization-e3.md)
- [Enterprise Compliance (E4)](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/enterprise-compliance-e4.md)
- [Enterprise Operations (E5)](https://github.com/muonroi/Muonroi.Docs/blob/main/docs/enterprise/enterprise-operations-e5.md)

## Troubleshooting

### "Connection string is not provided"

Ensure `DbType` matches the connection string key:

```json
{
  "DatabaseConfigs": {
    "DbType": "MySql",  // Must match key below
    "ConnectionStrings": {
      "MySqlConnectionString": "..."  // ✓ Correct key
    }
  }
}
```

### "The input is not a valid Base-64 string"

Set `"EnableEncryption": false` in appsettings.

### Migration errors

Always specify both `-p` (Kernel) and `--startup-project` (Host):

```bash
dotnet ef migrations add Init \
    -p ./src/Shared/MyModularApp.Kernel \
    --startup-project ./src/Host/MyModularApp.Host
```

### API slow on startup

Disable unused features in `FeatureFlags`.

### Module not loading

Check `Program.cs` for module registration:
```csharp
builder.Services.AddIdentityModule();
builder.Services.AddCatalogModule();
```

## Edition Notes

- Template package is MIT.
- Generated projects run in Free mode by default (`LicenseConfigs:LicenseFilePath = null`).
- If you enable premium modules (multi-tenant strict mode, RBAC+, rule-engine workflows, gRPC, message bus, distributed cache, audit trail), provide a paid Muonroi license with matching feature keys.

## License

MIT License. See [LICENSE.txt](LICENSE.txt) for details.
