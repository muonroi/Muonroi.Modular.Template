using Muonroi.BuildingBlock.External.Middleware;

namespace Muonroi.Modular.Host.Infrastructures;

public static class StartupExtensions
{
    public static void AddBaseTemplateServices(this IServiceCollection services,
        WebApplicationBuilder builder, Assembly assembly)
    {
        var configuration = builder.Configuration;

        // ===== CORE SERVICES =====
        _ = services.AddApplication(assembly);
        _ = services.AddInfrastructure(configuration);

        // ===== DATABASE (must be configured before DynamicPermission) =====
        _ = services.AddDbContextConfigure<BaseTemplateDbContext, Permission>(configuration);

        // ===== PERMISSION & AUTHORIZATION =====
        services.AddSingleton<MUONROI.Permissioning.AppPermissionProvider>();
        services.AddSingleton<IPermissionProvider>(sp =>
            sp.GetRequiredService<MUONROI.Permissioning.AppPermissionProvider>());
        _ = services.AddPermissionFilter<Permission>();
        _ = services.AddDynamicPermission<BaseTemplateDbContext>();

        // ===== API CONFIGURATION =====
        _ = services.RegisterServices(configuration);
        _ = services.SwaggerConfig(builder.Environment.ApplicationName);
        _ = services.AddScopeServices(typeof(BaseTemplateDbContext).Assembly);

        // ===== AUTHENTICATION & TOKEN =====
        _ = services.AddValidateBearerToken<BaseTemplateDbContext, Permission>(configuration);
        services.AddSingleton<Func<IServiceProvider, HttpContext, Task<MAuthenticateInfoContext>>>(_ =>
            async (provider, httpContext) =>
            {
                var validator = provider.GetService<IRefreshTokenValidator>();
                if (validator is null) return new MAuthenticateInfoContext(false);
                var result = await validator.ValidateAsync(httpContext);
                return result ?? new MAuthenticateInfoContext(false);
            });

        // ===== CACHING =====
        // Caching is configured via AddInfrastructure() based on CacheConfigs section
        // Supports: Memory, Redis, MultiLevel (see appsettings.Example.json)

        // ===== MULTI-TENANCY =====
        _ = services.AddTenantContext(configuration);

        // ===== CORS =====
        _ = services.AddCors(configuration);

        // ===== RULES ENGINE =====
        // Rules engine store is registered via AddInfrastructure() using RuleStore config.

        // ===== HEALTH CHECKS =====
        // Health checks are registered via AddInfrastructure() and exposed at /health via ConfigureEndpoints()
        // MessageBus adds Kafka/RabbitMQ health checks when enabled

        // ===== OPTIONAL FEATURES (controlled by FeatureFlags) =====
        var useGrpc = configuration.GetValue("FeatureFlags:UseGrpc", false);
        var useServiceDiscovery = configuration.GetValue("FeatureFlags:UseServiceDiscovery", false);
        var useMessageBus = configuration.GetValue("FeatureFlags:UseMessageBus", false);
        var useBackgroundJobs = configuration.GetValue("FeatureFlags:UseBackgroundJobs", false);

        // gRPC: Inter-service communication
        if (useGrpc) services.AddGrpcServer();

        // Service Discovery: Consul integration for microservices
        if (useServiceDiscovery) _ = services.AddServiceDiscovery(configuration, builder.Environment);

        // Message Bus: RabbitMQ/Kafka via MassTransit
        if (useMessageBus) _ = services.AddMessageBus(configuration, assembly);

        // Background Jobs: Hangfire/Quartz for scheduled tasks
        if (useBackgroundJobs) _ = services.AddBackgroundJobs(configuration);
    }

    public static async Task UseBaseTemplatePipelineAsync(this WebApplication app,
        WebApplicationBuilder builder, Assembly assembly)
    {
        var configuration = builder.Configuration;

        await app.UseServiceDiscoveryAsync(builder.Environment);
        _ = app.UseRouting();
        _ = app.UseCors("MAllowDomains");

        _ = app.UseDefaultMiddleware();
        _ = app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/swagger"),
            branch => { _ = branch.UseMiddleware<JwtMiddleware>(); });

        // Multi-tenant middleware (only if enabled)
        // Keep this after JwtMiddleware so tenant claim consistency can be validated.
        var multiTenantEnabled = configuration.GetValue("MultiTenantConfigs:Enabled", false);
        if (multiTenantEnabled)
        {
            _ = app.UseMiddleware<TenantContextMiddleware>();
        }

        _ = app.AddLocalization(assembly);
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        app.ConfigureEndpoints();

        var enableComplianceEndpoints = configuration.GetValue("EnterpriseOps:EnableComplianceEndpoints", false);
        var enableOperationsEndpoints = configuration.GetValue("EnterpriseOps:EnableOperationsEndpoints", false);
        if (enableComplianceEndpoints)
        {
            _ = TryMapEnterpriseEndpoint(
                app,
                "Muonroi.BuildingBlock.Shared.Compliance.MComplianceEndpointExtensions",
                "MapMComplianceEndpoints");
        }

        if (enableOperationsEndpoints)
        {
            _ = TryMapEnterpriseEndpoint(
                app,
                "Muonroi.BuildingBlock.Shared.Operations.MEnterpriseOperationsEndpointExtensions",
                "MapMEnterpriseOperationsEndpoints");
        }

        var ensureCreatedFallback = configuration.GetValue("FeatureFlags:UseEnsureCreatedFallback", false);
        if (ensureCreatedFallback)
        {
            using var scope = app.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<BaseTemplateDbContext>();
            var anyMigrations = (await ctx.Database.GetAppliedMigrationsAsync()).Any();
            if (!anyMigrations) await ctx.Database.EnsureCreatedAsync();
        }

        _ = app.MigrateDatabase<BaseTemplateDbContext>();
        await Seed.AdminRoleSeeder.SeedAsync<BaseTemplateDbContext>(app.Services);
    }

    private static bool TryMapEnterpriseEndpoint(WebApplication app, string extensionTypeName, string methodName)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.GetName().Name == "Muonroi.BuildingBlock");
        var extensionType = assembly?.GetType(extensionTypeName, throwOnError: false);
        var method = extensionType?.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(IEndpointRouteBuilder)],
            modifiers: null);
        if (method == null)
        {
            return false;
        }

        _ = method.Invoke(null, [app]);
        return true;
    }
}
