using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data;
using ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Base;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Data;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

public class MongoRepositoryIntegrationTests : IntegrationTestBase
{
    public MongoRepositoryIntegrationTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddAsync_WhenValidEntity_AddsToCollection()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();
        var history = new SubscriptionHistoryModel
        {
            Id = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            CreatedAt = DateTimeOffset.UtcNow,
            Steps = new List<SubscriptionHistoryStep>()
        };

        // Act
        await repository.AddAsync(history);

        // Assert - проверяем через ту же коллекцию, которую использует репозиторий
        var savedHistory = await repository.GetByIdAsync(history.Id);
        savedHistory.ShouldNotBeNull();
        savedHistory.Id.ShouldBe(history.Id);
        savedHistory.SubscriptionId.ShouldBe(history.SubscriptionId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        var history = new SubscriptionHistoryModel
        {
            Id = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            CreatedAt = DateTimeOffset.UtcNow,
            Steps = new List<SubscriptionHistoryStep>()
        };

        // Сохраняем через репозиторий, чтобы аудит сработал
        await repository.AddAsync(history);

        // Act
        var result = await repository.GetByIdAsync(history.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(history.Id);
        result.SubscriptionId.ShouldBe(history.SubscriptionId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityNotFound_ReturnsNull()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenEntitiesExist_ReturnsAllEntities()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        var histories = new List<SubscriptionHistoryModel>();
        for (int i = 0; i < 3; i++)
        {
            var history = new SubscriptionHistoryModel
            {
                Id = Guid.NewGuid(),
                SubscriptionId = Guid.NewGuid(),
                AbonentId = Guid.NewGuid(),
                TariffPlanId = Guid.NewGuid(),
                ActionType = SubscriptionActionType.Open,
                Status = SubscriptionActionStatus.Pending,
                StartDate = DateTimeOffset.UtcNow,
                TariffPlanName = $"Test Tariff {i}",
                ServiceName = $"Test Service {i}",
                ContractNumber = $"CONTRACT-00{i}",
                CreatedAt = DateTimeOffset.UtcNow,
                Steps = new List<SubscriptionHistoryStep>()
            };
            histories.Add(history);
        }

        // Сохраняем через репозиторий
        foreach (var history in histories)
        {
            await repository.AddAsync(history);
        }

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityExists_UpdatesEntity()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        var history = new SubscriptionHistoryModel
        {
            Id = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            CreatedAt = DateTimeOffset.UtcNow,
            Steps = new List<SubscriptionHistoryStep>()
        };

        await repository.AddAsync(history);

        // Act - обновляем через репозиторий
        history.Status = SubscriptionActionStatus.Completed;
        await repository.UpdateAsync(history);

        // Assert
        var updatedHistory = await repository.GetByIdAsync(history.Id);
        updatedHistory.ShouldNotBeNull();
        updatedHistory.Status.ShouldBe(SubscriptionActionStatus.Completed);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityExists_DeletesEntity()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        var history = new SubscriptionHistoryModel
        {
            Id = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            CreatedAt = DateTimeOffset.UtcNow,
            Steps = new List<SubscriptionHistoryStep>()
        };

        await repository.AddAsync(history);

        // Act
        await repository.DeleteAsync(history.Id);

        // Assert
        var deletedHistory = await repository.GetByIdAsync(history.Id);
        deletedHistory.ShouldBeNull();
    }

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ReturnsFilteredEntities()
    {
        // Arrange
        using var scope = Fixture.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMongoRepository<SubscriptionHistoryModel>>();

        var targetSubscriptionId = Guid.NewGuid();
        var histories = new List<SubscriptionHistoryModel>();
        for (int i = 0; i < 3; i++)
        {
            var history = new SubscriptionHistoryModel
            {
                Id = Guid.NewGuid(),
                SubscriptionId = i == 1 ? targetSubscriptionId : Guid.NewGuid(),
                AbonentId = Guid.NewGuid(),
                TariffPlanId = Guid.NewGuid(),
                ActionType = SubscriptionActionType.Open,
                Status = SubscriptionActionStatus.Pending,
                StartDate = DateTimeOffset.UtcNow,
                TariffPlanName = $"Test Tariff {i}",
                ServiceName = $"Test Service {i}",
                ContractNumber = $"CONTRACT-00{i}",
                CreatedAt = DateTimeOffset.UtcNow,
                Steps = new List<SubscriptionHistoryStep>()
            };
            histories.Add(history);
        }

        foreach (var history in histories)
        {
            await repository.AddAsync(history);
        }

        // Act
        var result = await repository.GetWhereAsync(h => h.SubscriptionId == targetSubscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result.First().SubscriptionId.ShouldBe(targetSubscriptionId);
    }
}