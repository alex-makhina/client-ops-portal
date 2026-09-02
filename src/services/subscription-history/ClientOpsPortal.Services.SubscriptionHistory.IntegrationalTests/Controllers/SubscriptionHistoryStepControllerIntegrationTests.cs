using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Base;
using MongoDB.Driver;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClientOpsPortal.Services.SubscriptionHistory.IntegrationalTests.Controllers;

public class SubscriptionHistoryStepControllerIntegrationTests : IntegrationTestBase
{
    public SubscriptionHistoryStepControllerIntegrationTests(TestFixture fixture) : base(fixture)
    {
        ClearDatabaseAsync().Wait();
    }

    [Fact]
    public async Task GetAll_WhenNoSteps_ReturnsEmptyList()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/subscriptionhistorystep");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryStepDto>>();
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenStepsExist_ReturnsSteps()
    {
        // Arrange
        await SeedStepAsync(3);
        var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/subscriptionhistorystep");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryStepDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetById_WhenStepExists_ReturnsStep()
    {
        // Arrange
        var step = await SeedStepAsync();
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistorystep/{step.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(step.Id);
        result.SubscriptionHistoryId.ShouldBe(step.SubscriptionHistoryId);
    }

    [Fact]
    public async Task GetById_WhenStepNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistorystep/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByHistory_WhenStepsExist_ReturnsSteps()
    {
        // Arrange
        var historyId = Guid.NewGuid();
        await SeedStepAsync(2, historyId);
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/subscriptionhistorystep/by-history/{historyId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<SubscriptionHistoryStepDto>>();
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.All(s => s.SubscriptionHistoryId == historyId).ShouldBeTrue();
    }

    [Fact]
    public async Task Create_WhenValidDto_ReturnsCreatedStep()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = "Test message"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/subscriptionhistorystep", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>();
        result.ShouldNotBeNull();
        result.SubscriptionHistoryId.ShouldBe(createDto.SubscriptionHistoryId);
        result.Status.ShouldBe(createDto.Status);
        result.Message.ShouldBe(createDto.Message);

        var savedStep = await GetStepCollection()
            .Find(s => s.Id == result.Id)
            .FirstOrDefaultAsync();

        savedStep.ShouldNotBeNull();
        savedStep.SubscriptionHistoryId.ShouldBe(createDto.SubscriptionHistoryId);
    }

    [Fact]
    public async Task Create_WhenInvalidDto_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateSubscriptionHistoryStepDto
        {
            SubscriptionHistoryId = Guid.Empty,
            Status = SubscriptionActionStatus.Pending
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/subscriptionhistorystep", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenStepExists_ReturnsUpdatedStep()
    {
        // Arrange
        var step = await SeedStepAsync();
        var client = CreateClient();
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed,
            Message = "Updated message"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/subscriptionhistorystep/{step.Id}", updateDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>();
        result.ShouldNotBeNull();
        result.Status.ShouldBe(SubscriptionActionStatus.Completed);
        result.Message.ShouldBe("Updated message");

        var savedStep = await GetStepCollection()
            .Find(s => s.Id == step.Id)
            .FirstOrDefaultAsync();

        savedStep.ShouldNotBeNull();
        savedStep.Status.ShouldBe(SubscriptionActionStatus.Completed);
        savedStep.Message.ShouldBe("Updated message");
    }

    [Fact]
    public async Task Update_WhenStepNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var updateDto = new UpdateSubscriptionHistoryStepDto
        {
            Status = SubscriptionActionStatus.Completed
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/subscriptionhistorystep/{Guid.NewGuid()}", updateDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenStepExists_ReturnsNoContent()
    {
        // Arrange
        var step = await SeedStepAsync();
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/subscriptionhistorystep/{step.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var savedStep = await GetStepCollection()
            .Find(s => s.Id == step.Id)
            .FirstOrDefaultAsync();

        savedStep.ShouldBeNull();
    }

    [Fact]
    public async Task Delete_WhenStepNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/subscriptionhistorystep/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<SubscriptionHistoryStep> SeedStepAsync(Guid? historyId = null)
    {
        var step = new SubscriptionHistoryStep
        {
            Id = Guid.NewGuid(),
            SubscriptionHistoryId = historyId ?? Guid.NewGuid(),
            Status = SubscriptionActionStatus.Pending,
            Message = "Test message",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test-user"
        };

        await GetStepCollection().InsertOneAsync(step);
        return step;
    }

    private async Task SeedStepAsync(int count, Guid? historyId = null)
    {
        for (int i = 0; i < count; i++)
        {
            await SeedStepAsync(historyId);
        }
    }
}