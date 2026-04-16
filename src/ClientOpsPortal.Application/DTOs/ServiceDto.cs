using ClientOpsPortal.Application.DTOs.Common;

namespace ClientOpsPortal.Application.DTOs
{
    public class ServiceDto : AuditableDto
    {
        public required string Name { get; set; } 
        public required string Description { get; set; } 
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class ServiceShortDataDto 
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTimeOffset.UtcNow;
    }

    public class ServiceFullDataDto : BaseDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public List<TariffPlanDto> TariffPlans { get; set; } = new();
    }

    public class CreateServiceDto 
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public List<TariffPlanDto> TariffPlans { get; set; } = new();
    }

    public class UpdateServiceDto
    {
        public string? Name { get; set; }           
        public string? Description { get; set; }  
        public DateTimeOffset? EndDate { get; set; }
        public List<UpdateTariffPlanFromServiceDto>? TariffPlans { get; set; }  
    }
}
