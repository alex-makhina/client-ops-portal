using ClientOpsPortal.Application.DTOs.Common;

namespace ClientOpsPortal.Application.DTOs
{
    public class TariffPlanDto : BaseDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class TariffPlanShortDataDto : BaseDto
    {
        public required string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateTariffPlanDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required Guid ServiceId { get; set; }
        public required DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class UpdateTariffPlanDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class UpdateTariffPlanFromServiceDto
    {
        public Guid Id { get; set; } 
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
