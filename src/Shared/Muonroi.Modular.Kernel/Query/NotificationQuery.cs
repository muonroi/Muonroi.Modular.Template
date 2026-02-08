using Muonroi.BuildingBlock.Shared.License;
namespace Muonroi.Modular.Kernel.Query
{
    public class NotificationQuery(BaseTemplateDbContext dbContext, MAuthenticateInfoContext authContext, ILicenseGuard licenseGuard)
        : MQuery<NotificationEntity>(dbContext, authContext, licenseGuard), INotificationQuery
    {
    }
}




