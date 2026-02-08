namespace Muonroi.Modular.Host.Application.Commands.Container;

public sealed record CreateContainerCommand(string Code) : IRequest<MResponse<string>>;

