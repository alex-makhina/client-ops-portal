using ClientOpsPortal.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;

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
        public Guid Id {get; set;}
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
        [Required(ErrorMessage = "Название услуги обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название услуги должно содержать от 3 до 100 символов")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Описание услуги обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание услуги должно содержать от 10 до 500 символов")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTimeOffset BeginDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public List<CreateTariffPlanDto> TariffPlans { get; set; } = new();
    }

    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "Название услуги обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название услуги должно содержать от 3 до 100 символов")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Описание услуги обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание услуги должно содержать от 10 до 500 символов")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTimeOffset BeginDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public List<UpdateTariffPlanFromServiceDto>? TariffPlans { get; set; }  
    }
}
