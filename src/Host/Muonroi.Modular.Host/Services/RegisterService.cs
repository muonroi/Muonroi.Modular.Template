namespace Muonroi.Modular.Host.Services
{
    public static class RegisterService
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            _ = configuration;

            services.AddRuleEngine<int>();
            services.AddRulesFromAssemblies(typeof(RegisterService).Assembly);

            return services;
        }
    }

}

