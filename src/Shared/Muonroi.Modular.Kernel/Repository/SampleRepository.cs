using Muonroi.BuildingBlock.Shared.License;
namespace Muonroi.Modular.Kernel.Repository
{
    public class SampleRepository(BaseTemplateDbContext dbContext, MAuthenticateInfoContext authContext, ILicenseGuard licenseGuard)
        : MRepository<SampleEntity>(dbContext, authContext, licenseGuard), ISampleRepository
    {
    }
}




