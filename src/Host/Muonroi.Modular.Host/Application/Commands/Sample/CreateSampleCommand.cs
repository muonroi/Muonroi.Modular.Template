

namespace Muonroi.Modular.Host.Application.Commands.Sample
{
    public class CreateSampleCommand : IRequest<MResponse<SampleDto>>
    {
        public string Name { get; set; } = string.Empty;
    }
}

