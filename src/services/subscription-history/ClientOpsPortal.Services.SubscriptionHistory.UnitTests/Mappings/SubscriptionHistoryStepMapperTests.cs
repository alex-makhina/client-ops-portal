using AutoBogus;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Mappings;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Mappings;

public class SubscriptionHistoryStepMapperTests
{
    [Fact]
    public void ToSubscriptionHistoryStepDto_WhenStepExists_ReturnsDto()
    {
        // Arrange
        var step = CreateSubscriptionHistoryStepEntity();

        // Act
        var dto = step.ToSubscriptionHistoryStepDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(step.Id);
        dto.SubscriptionHistoryId.ShouldBe(step.SubscriptionHistoryId);
        dto.Status.ShouldBe(step.Status);
        dto.Message.ShouldBe(step.Message);
        dto.CreatedAt.ShouldBe(step.CreatedAt);
        dto.CreatedBy.ShouldBe(step.CreatedBy);
    }

    [Fact]
    public void ToEntity_FromCreateDto_ReturnsEntity()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryStepDto();

        // Act
        var entity = createDto.ToEntity();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.SubscriptionHistoryId.ShouldBe(createDto.SubscriptionHistoryId);
        entity.Status.ShouldBe(createDto.Status);
        entity.Message.ShouldBe(createDto.Message);
        entity.CreatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void ToEntity_WhenCreateDtoHasNullMessage_SetsNull()
    {
        // Arrange
        var createDto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = null
        };

        // Act
        var entity = createDto.ToEntity();

        // Assert
        entity.Message.ShouldBeNull();
    }

    [Fact]
    public void UpdateEntity_WhenStatusHasValue_UpdatesStatus()
    {
        // Arrange
        var entity = CreateSubscriptionHistoryStepEntity();
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed,
            Message = null
        };

        // Act
        updateDto.UpdateEntity(entity);

        // Assert
        entity.Status.ShouldBe(SubscriptionActionStatus.Completed);
    }

    [Fact]
    public void UpdateEntity_WhenStatusIsNull_DoesNotUpdateStatus()
    {
        // Arrange
        var entity = CreateSubscriptionHistoryStepEntity();
        var originalStatus = entity.Status;
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = null,
            Message = null
        };

        // Act
        updateDto.UpdateEntity(entity);

        // Assert
        entity.Status.ShouldBe(originalStatus);
    }

    [Fact]
    public void UpdateEntity_WhenMessageIsNotNull_UpdatesMessage()
    {
        // Arrange
        var entity = CreateSubscriptionHistoryStepEntity();
        var newMessage = "Обновленное сообщение";
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = null,
            Message = newMessage
        };

        // Act
        updateDto.UpdateEntity(entity);

        // Assert
        entity.Message.ShouldBe(newMessage);
    }

    [Fact]
    public void UpdateEntity_WhenMessageIsNull_DoesNotUpdateMessage()
    {
        // Arrange
        var entity = CreateSubscriptionHistoryStepEntity();
        var originalMessage = entity.Message;
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = null,
            Message = null
        };

        // Act
        updateDto.UpdateEntity(entity);

        // Assert
        entity.Message.ShouldBe(originalMessage);
    }

    private static SubscriptionHistoryStep CreateSubscriptionHistoryStepEntity()
    {
        return new AutoFaker<SubscriptionHistoryStep>().Generate();
    }

    private static CreateSubscriptionHistoryStepDto CreateCreateSubscriptionHistoryStepDto()
    {
        return new AutoFaker<CreateSubscriptionHistoryStepDto>().Generate();
    }
}