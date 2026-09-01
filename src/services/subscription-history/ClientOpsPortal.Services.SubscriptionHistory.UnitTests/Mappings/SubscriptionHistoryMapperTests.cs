using AutoBogus;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Mappings;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.UnitTests.Mappings;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

public class SubscriptionHistoryMapperTests
{
    [Fact]
    public void ToSubscriptionHistoryDto_WhenHistoryExists_ReturnsDto()
    {
        // Arrange
        var history = CreateSubscriptionHistoryEntity();

        // Act
        var dto = history.ToSubscriptionHistoryDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(history.Id);
        dto.SubscriptionId.ShouldBe(history.SubscriptionId);
        dto.ActionType.ShouldBe(history.ActionType);
        dto.Status.ShouldBe(history.Status);
        dto.TariffPlanId.ShouldBe(history.TariffPlanId);
        dto.StartDate.ShouldBe(history.StartDate);
        dto.CreatedAt.ShouldBe(history.CreatedAt);
        dto.CreatedBy.ShouldBe(history.CreatedBy);
        dto.UpdatedAt.ShouldBe(history.UpdatedAt);
        dto.UpdatedBy.ShouldBe(history.UpdatedBy);
    }

    [Fact]
    public void ToSubscriptionHistoryDto_WhenHistoryHasSteps_MapsSteps()
    {
        // Arrange
        var history = CreateSubscriptionHistoryEntity();
        history.Steps = new List<SubscriptionHistoryStep>
        {
            new AutoFaker<SubscriptionHistoryStep>().Generate(),
            new AutoFaker<SubscriptionHistoryStep>().Generate()
        };

        // Act
        var dto = history.ToSubscriptionHistoryDto();

        // Assert
        dto.Steps.ShouldNotBeNull();
        dto.Steps.Count.ShouldBe(2);
        dto.Steps.ShouldBe(history.Steps);
    }

    [Fact]
    public void ToEntity_FromCreateDto_ReturnsEntity()
    {
        // Arrange
        var createDto = CreateCreateSubscriptionHistoryDto();

        // Act
        var entity = createDto.ToEntity();

        // Assert
        entity.ShouldNotBeNull();
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.SubscriptionId.ShouldBe(createDto.SubscriptionId);
        entity.ActionType.ShouldBe(createDto.ActionType);
        entity.Status.ShouldBe(createDto.Status);
        entity.TariffPlanId.ShouldBe(createDto.TariffPlanId);
        entity.TariffPlanName.ShouldBe(createDto.TariffPlanName);
        entity.ServiceName.ShouldBe(createDto.ServiceName);
        entity.ContractNumber.ShouldBe(createDto.ContractNumber);
        entity.AbonentId.ShouldBe(createDto.AbonentId);
        entity.StartDate.ShouldBe(createDto.StartDate);
        entity.CreatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void ToEntity_WhenCreateDtoHasNoSteps_CreatesEmptyStepsList()
    {
        // Arrange
        var createDto = new AutoFaker<CreateSubscriptionHistoryDto>()
            .RuleFor(dto => dto.Steps, _ => null)
            .Generate();

        // Act
        var entity = createDto.ToEntity();

        // Assert
        entity.Steps.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateEntity_FromUpdateDto_UpdatesStatus()
    {
        // Arrange
        var entity = CreateSubscriptionHistoryEntity();
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        // Act
        updateDto.UpdateEntity(entity);

        // Assert
        entity.Status.ShouldBe(SubscriptionActionStatus.Completed);
    }

    [Fact]
    public void ToSubscriptionHistoryFullDto_WhenHistoryExists_ReturnsFullDto()
    {
        // Arrange
        var history = CreateSubscriptionHistoryEntity();
        history.Steps = new List<SubscriptionHistoryStep>
        {
            new AutoFaker<SubscriptionHistoryStep>().Generate(),
            new AutoFaker<SubscriptionHistoryStep>().Generate()
        };

        // Act
        var dto = history.ToSubscriptionHistoryFullDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(history.Id);
        dto.SubscriptionId.ShouldBe(history.SubscriptionId);
        dto.ActionType.ShouldBe(history.ActionType);
        dto.Status.ShouldBe(history.Status);
        dto.TariffPlanId.ShouldBe(history.TariffPlanId);
        dto.TariffPlanName.ShouldBe(history.TariffPlanName);
        dto.ServiceName.ShouldBe(history.ServiceName);
        dto.ContractNumber.ShouldBe(history.ContractNumber);
        dto.AbonentId.ShouldBe(history.AbonentId);
        dto.StartDate.ShouldBe(history.StartDate);
        dto.Steps.ShouldNotBeNull();
        dto.Steps.Count.ShouldBe(2);
    }

    private static SubscriptionHistoryModel CreateSubscriptionHistoryEntity()
    {
        return new AutoFaker<SubscriptionHistoryModel>()
            .RuleFor(h => h.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }

    private static CreateSubscriptionHistoryDto CreateCreateSubscriptionHistoryDto()
    {
        return new AutoFaker<CreateSubscriptionHistoryDto>()
            .RuleFor(dto => dto.Steps, _ => new List<SubscriptionHistoryStep>())
            .Generate();
    }
}