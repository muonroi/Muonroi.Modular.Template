

namespace Muonroi.Modular.Host.Application.Commands.Login
{
    public class LoginCommand : IRequest<MResponse<LoginResponseModel>>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

