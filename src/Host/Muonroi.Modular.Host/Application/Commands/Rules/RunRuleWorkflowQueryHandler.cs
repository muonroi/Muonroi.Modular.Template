namespace Muonroi.Modular.Host.Application.Commands.Rules;

public sealed class RunRuleWorkflowQueryHandler(IMRuleWorkflowRunner<int> workflowRunner)
    : IRequestHandler<RunRuleWorkflowQuery, MResponse<RuleWorkflowSampleResult>>
{
    private readonly IMRuleWorkflowRunner<int> _workflowRunner = workflowRunner;

    public async Task<MResponse<RuleWorkflowSampleResult>> Handle(
        RunRuleWorkflowQuery request,
        CancellationToken cancellationToken)
    {
        var workflow = BuildWorkflow(request.Mode);
        var result = await _workflowRunner.ExecuteAsync(request.Value, workflow, cancellationToken).ConfigureAwait(false);

        RuleWorkflowSampleResult model = new()
        {
            WorkflowName = result.WorkflowName,
            Input = request.Value,
            EffectiveMode = request.Mode?.ToString() ?? RuleExecutionMode.Rules.ToString(),
            ExecutedSteps = [.. result.ExecutedSteps]
        };

        foreach (var (key, value) in result.Facts.AsReadOnly())
        {
            model.Facts[key] = value;
        }

        foreach (var (key, value) in result.State)
        {
            model.State[key] = value;
        }

        return new MResponse<RuleWorkflowSampleResult> { Result = model };
    }

    private static MRuleWorkflowDefinition<int> BuildWorkflow(RuleExecutionMode? modeOverride)
    {
        return new MRuleWorkflowDefinition<int>(
            "number-classification-workflow",
            "start",
            [
                MRuleWorkflowStep<int>.Start("start", "evaluate-rules"),
                MRuleWorkflowStep<int>.RuleTask(
                    "evaluate-rules",
                    "route",
                    modeOverride,
                    (ctx, _) =>
                    {
                        ctx.SetState("traditionalPathUsed", true);
                        ctx.Facts["mode"] = "traditional";
                        ctx.Facts["even"] = ctx.Context % 2 == 0;
                        ctx.Facts["square"] = ctx.Context * ctx.Context;
                        return Task.CompletedTask;
                    }),
                MRuleWorkflowStep<int>.ExclusiveGateway("route", (ctx, _) =>
                {
                    var isEven = ctx.Facts.TryGet<bool>("even", out var even) && even;
                    return Task.FromResult(isEven ? "even-branch" : "odd-branch");
                }),
                MRuleWorkflowStep<int>.ServiceTask("even-branch", "end", (ctx, _) =>
                {
                    ctx.Facts["workflowDecision"] = "approve";
                    ctx.Facts["riskLevel"] = "low";
                    return Task.CompletedTask;
                }),
                MRuleWorkflowStep<int>.ServiceTask("odd-branch", "end", (ctx, _) =>
                {
                    ctx.Facts["workflowDecision"] = "manual-review";
                    ctx.Facts["riskLevel"] = "medium";
                    return Task.CompletedTask;
                }),
                MRuleWorkflowStep<int>.End("end")
            ]);
    }
}

