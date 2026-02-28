# Rule Engine Quick Start (Modular Template)

## 1. Configuration

Add to host `appsettings`:

```json
{
  "RuleStore": {
    "RootPath": "rules",
    "UseContentRoot": true
  }
}
```

## 2. Registration

In host startup:

```csharp
builder.Services.AddRuleEngineStore(builder.Configuration);
builder.Services.AddRuleEngine<ContainerContext>(o => o.ExecutionMode = RuleExecutionMode.Rules)
    .AddRule<ContainerValidationRule>()
    .AddRule<ContainerExistenceRule>()
    .AddHook<LoggingHook<ContainerContext>>();
```

## 3. Template rule samples

- `src/Host/Muonroi.Modular.Host/Rules/B2B/B2BRegistrationRules.cs`
- `src/Host/Muonroi.Modular.Host/Rules/ContainerValidationRule.cs`
- `src/Host/Muonroi.Modular.Host/Rules/ContainerExistenceRule.cs`
- `src/Host/Muonroi.Modular.Host/Rules/LoggingHook.cs`

## 4. Runtime mode switch

```csharp
[RuleMode(RuleExecutionMode.Shadow)]
public class OrdersController : ControllerBase
{
}
```

## 5. Code-first extraction CLI

```bash
muonroi-rule extract --source ./Modules/Orders/OrderService.cs --output ./Rules/Generated --namespace MyApp.Rules
muonroi-rule register --rules ./Rules/Generated --output ./Rules/Generated/RuleRegistration.g.cs
```

## 6. References

- https://github.com/muonroi/Muonroi.Docs/blob/main/docs/rule-engine-guide.md
- https://github.com/muonroi/Muonroi.Docs/blob/main/docs/rule-engine-advanced-patterns.md
