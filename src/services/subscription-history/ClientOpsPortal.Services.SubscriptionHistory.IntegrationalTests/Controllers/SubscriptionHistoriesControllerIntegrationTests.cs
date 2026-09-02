using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Base;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Controllers;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

public class SubscriptionHistoriesControllerIntegrationTests : IntegrationTestBase
{
    public SubscriptionHistoriesControllerIntegrationTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAll_WhenNoHistories_ReturnsEmptyList()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/subscriptionhistories");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryDto>>();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenHistoriesExist_ReturnsHistories()
    {
        // Arrange
        await SeedHistoryAsync(3);
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/subscriptionhistories");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetById_WhenHistoryExists_ReturnsHistory()
    {
        // Arrange
        var history = await SeedHistoryAsync();
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistories/{history.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(history.Id);
        result.SubscriptionId.ShouldBe(history.SubscriptionId);
    }

    [Fact]
    public async Task GetById_WhenHistoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBySubscription_WhenHistoriesExist_ReturnsHistories()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        await SeedHistoryAsync(2, subscriptionId: subscriptionId);
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistories/by-subscription/{subscriptionId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.All(h => h.SubscriptionId == subscriptionId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetByAbonent_WhenHistoriesExist_ReturnsHistories()
    {
        // Arrange
        var abonentId = Guid.NewGuid();
        await SeedHistoryAsync(2, abonentId: abonentId);
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistories/by-abonent/{abonentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryFullDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.All(h => h.AbonentId == abonentId).ShouldBeTrue();
    }

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedHistory()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.NewGuid(),
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            Steps = new List<SubscriptionHistoryStep>()
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/subscriptionhistories", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>();
        result.ShouldNotBeNull();
        result.SubscriptionId.ShouldBe(createDto.SubscriptionId);
        result.ActionType.ShouldBe(createDto.ActionType);
        result.Status.ShouldBe(createDto.Status);

        var getResponse = await client.GetAsync($"/api/v1/subscriptionhistories/{result.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var savedHistory = await getResponse.Content.ReadFromJsonAsync<SubscriptionHistoryDto>();
        savedHistory.ShouldNotBeNull();
        savedHistory.SubscriptionId.ShouldBe(createDto.SubscriptionId);
    }

    [Fact]
    public async Task Create_WhenInvalidDto_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = Guid.Empty,
            AbonentId = Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/subscriptionhistories", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenHistoryExists_ReturnsUpdatedHistory()
    {
        // Arrange
        var history = await SeedHistoryAsync();
        var client = CreateClient();
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/subscriptionhistories/{history.Id}", updateDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>();
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SubscriptionActionStatus.Completed);

        var getResponse = await client.GetAsync($"/api/v1/subscriptionhistories/{history.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedHistory = await getResponse.Content.ReadFromJsonAsync<SubscriptionHistoryDto>();
        updatedHistory.ShouldNotBeNull();
        updatedHistory.Status.ShouldBe(SubscriptionActionStatus.Completed);
    }

    [Fact]
    public async Task Update_WhenHistoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var updateDto = new UpdateSubscriptionHistoryDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/subscriptionhistories/{Guid.NewGuid()}", updateDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenHistoryExists_ReturnsNoContent()
    {
        // Arrange
        var history = await SeedHistoryAsync();
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/subscriptionhistories/{history.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/subscriptionhistories/{history.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenHistoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/subscriptionhistories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<SubscriptionHistoryModel> SeedHistoryAsync(Guid? subscriptionId = null, Guid? abonentId = null)
    {
        using var scope = Fixture.Services.CreateScope();
        var historyService = scope.ServiceProvider.GetRequiredService<SubscriptionHistoryService>();

        var createDto = new CreateSubscriptionHistoryDto
        {
            SubscriptionId = subscriptionId ?? Guid.NewGuid(),
            AbonentId = abonentId ?? Guid.NewGuid(),
            TariffPlanId = Guid.NewGuid(),
            ActionType = SubscriptionActionType.Open,
            Status = SubscriptionActionStatus.Pending,
            StartDate = DateTimeOffset.UtcNow,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            Steps = new List<SubscriptionHistoryStep>()
        };

        var historyDto = await historyService.CreateSubscriptionHistoryAsync(createDto, CancellationToken.None);

        return new SubscriptionHistoryModel
        {
            Id = historyDto.Id,
            SubscriptionId = historyDto.SubscriptionId,
            AbonentId = abonentId ?? Guid.NewGuid(),
            TariffPlanId = historyDto.TariffPlanId,
            ActionType = historyDto.ActionType,
            Status = historyDto.Status,
            StartDate = historyDto.StartDate,
            TariffPlanName = "Test Tariff",
            ServiceName = "Test Service",
            ContractNumber = "CONTRACT-001",
            CreatedAt = historyDto.CreatedAt,
            Steps = new List<SubscriptionHistoryStep>()
        };
    }

    private async Task SeedHistoryAsync(int count, Guid? subscriptionId = null, Guid? abonentId = null)
    {
        for (int i = 0; i < count; i++)
        {
            await SeedHistoryAsync(subscriptionId, abonentId);
        }
    }
}