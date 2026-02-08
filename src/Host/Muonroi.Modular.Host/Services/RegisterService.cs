namespace Muonroi.Modular.Host.Services
{
    public static class RegisterService
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            services
                .AddRuleEngine()
                .AddRulesFromAssemblies(typeof(RegisterService).Assembly);

            return services;
        }
    }

}

