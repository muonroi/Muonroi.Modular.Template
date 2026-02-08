using Muonroi.BuildingBlock.Shared.License;

namespace Muonroi.Modular.Kernel.Persistence
{
    public class BaseTemplateDbContext : MDbContext
    {
        public DbSet<SampleEntity> Samples { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }

        public BaseTemplateDbContext(DbContextOptions options, IMediator mediator,
            ILicenseGuard? licenseGuard = null, ILogger<BaseTemplateDbContext>? logger = null)
            : base(options, mediator, licenseGuard, logger)
        { }
    }
}

