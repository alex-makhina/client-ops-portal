namespace ClientOpsPortal.Services.Directory.Services
{
    public class ServiceCacheOptions
    {
        public int ActiveServicesMinutes { get; set; } = 120;
        public int ServiceByIdMinutes { get; set; } = 120;
    }
}
