using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Mappings
{
    public static class SubscriptionHistoryStepMapper
    {
        public static SubscriptionHistoryStepDto ToSubscriptionHistoryStepDto(this SubscriptionHistoryStep step)
        {
            return new SubscriptionHistoryStepDto
            {
                Id = step.Id,
                SubscriptionHistoryId = step.SubscriptionHistoryId,
                Status = step.Status,
                Message = step.Message,
                CreatedAt = step.CreatedAt,
                CreatedBy = step.CreatedBy
            };
        }

        public static SubscriptionHistoryStep ToEntity(this CreateSubscriptionHistoryStepDto createDto)
        {
            return new SubscriptionHistoryStep
            {
                Id = Guid.NewGuid(),
                SubscriptionHistoryId = createDto.SubscriptionHistoryId,
                Status = createDto.Status,
                Message = createDto.Message,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public static void UpdateEntity(this UpdateSubscriptionHistoryStepDto updateDto, SubscriptionHistoryStep entity)
        {
            if (updateDto.Status.HasValue)
                entity.Status = updateDto.Status.Value;

            if (updateDto.Message != null)
                entity.Message = updateDto.Message;
        }
    }
}