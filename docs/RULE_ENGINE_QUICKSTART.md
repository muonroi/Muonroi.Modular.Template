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

## 7. Workflow Adapter sample API

Template includes a BPMN-like workflow adapter sample:

- `GET /api/v1/rules/workflow/{value}?mode=Rules`
- `GET /api/v1/rules/workflow/{value}?mode=Traditional`

Example:

```bash
curl "http://localhost:5000/api/v1/rules/workflow/8?mode=Rules"
```

Expected response fields:

- `result.executedSteps`
- `result.facts.workflowDecision`
- `result.facts.riskLevel`
- `result.state.traditionalPathUsed` (when mode is `Traditional`)
