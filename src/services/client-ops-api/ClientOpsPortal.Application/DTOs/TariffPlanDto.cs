using ClientOpsPortal.Application.DTOs.Common;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Название тарифного плана обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название тарифного плана должно содержать от 3 до 100 символов")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Описание тарифного плана обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание тарифного плана должно содержать от 10 до 500 символов")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0, 999, ErrorMessage = "Цена должна быть от 0 до 999")]
        public required decimal Price { get; set; }

        public required Guid ServiceId { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        public required DateTimeOffset BeginDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }

    public class UpdateTariffPlanDto
    {
        [Required(ErrorMessage = "Название тарифного плана обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название тарифного плана должно содержать от 3 до 100 символов")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Описание тарифного плана обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание тарифного плана должно содержать от 10 до 500 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0, 999, ErrorMessage = "Цена должна быть от 0 до 999")]
        public decimal? Price { get; set; }

        public DateTimeOffset? BeginDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }

    public class UpdateTariffPlanFromServiceDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название тарифного плана обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название тарифного плана должно содержать от 3 до 100 символов")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Описание тарифного плана обязательно")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Описание тарифного плана должно содержать от 10 до 500 символов")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0, 999, ErrorMessage = "Цена должна быть от 0 до 999")]
        public decimal Price { get; set; }

        public DateTimeOffset BeginDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }
}
