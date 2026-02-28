namespace Muonroi.Modular.Host.Application.Models;

public sealed class RuleWorkflowSampleResult
{
    public string WorkflowName { get; init; } = string.Empty;
    public int Input { get; init; }
    public string EffectiveMode { get; init; } = string.Empty;
    public string[] ExecutedSteps { get; init; } = [];
    public Dictionary<string, object?> Facts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, object?> State { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

