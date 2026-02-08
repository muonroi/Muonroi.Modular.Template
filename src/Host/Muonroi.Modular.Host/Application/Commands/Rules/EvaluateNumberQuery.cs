namespace Muonroi.Modular.Host.Application.Commands.Rules;

public sealed record EvaluateNumberQuery(int Value) : IRequest<MResponse<FactBag>>;

