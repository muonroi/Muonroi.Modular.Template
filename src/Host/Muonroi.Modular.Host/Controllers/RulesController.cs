namespace Muonroi.Modular.Host.Controllers;

public class RulesController(IMediator mediator, ILogger logger, IMapper mapper)
    : MControllerBase(mediator, logger, mapper)
{
    [HttpGet("evaluate/{value:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Evaluate(int value, CancellationToken cancellationToken)
    {
        var response = await Mediator
            .Send(new EvaluateNumberQuery(value), cancellationToken)
            .ConfigureAwait(false);
        return response.GetActionResult();
    }

    [HttpGet("workflow/{value:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> EvaluateWorkflow(
        int value,
        [FromQuery] RuleExecutionMode? mode,
        CancellationToken cancellationToken)
    {
        var response = await Mediator
            .Send(new RunRuleWorkflowQuery(value, mode), cancellationToken)
            .ConfigureAwait(false);
        return response.GetActionResult();
    }
}

