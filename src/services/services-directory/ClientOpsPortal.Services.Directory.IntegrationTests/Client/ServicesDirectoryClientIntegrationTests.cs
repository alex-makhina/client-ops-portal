using AutoBogus;
using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClientOpsPortal.Services.Directory.IntegrationTests.Client;

public class ServicesDirectoryClientIntegrationTests : IAsyncLifetime
{
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;
    private readonly ServicesDirectoryClient _client;

    public ServicesDirectoryClientIntegrationTests()
    {
        var builder = new WebHostBuilder()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddControllers();
                services.AddRouting();
                services.AddLogging();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/api/v1/services", async context =>
                    {
                        var services = CreateServiceDtoList(3);
                        await WriteJsonResponse(context, services);
                    });

                    endpoints.MapGet("/api/v1/services/{id}", async context =>
                    {
                        var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString()!);
                        var service = CreateServiceDto(id);
                        await WriteJsonResponse(context, service);
                    });

                    endpoints.MapGet("/api/v1/services/full/{id}", async context =>
                    {
                        var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString()!);
                        var fullService = CreateServiceFullDataDto(id);
                        await WriteJsonResponse(context, fullService);
                    });

                    endpoints.MapGet("/api/v1/services/active", async context =>
                    {
                        var services = CreateServiceShortDataDtoList(2);
                        await WriteJsonResponse(context, services);
                    });

                    endpoints.MapPost("/api/v1/services", async context =>
                    {
                        var dto = await context.Request.ReadFromJsonAsync<CreateServiceDto>();
                        var created = new ServiceDto
                        {
                            Id = Guid.NewGuid(),
                            Name = dto?.Name ?? "Default Service",
                            Description = dto?.Description ?? "Default Description",
                            BeginDate = dto?.BeginDate ?? DateTimeOffset.UtcNow,
                            EndDate = null,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = "test-user"
                        };
                        context.Response.StatusCode = 201;
                        await WriteJsonResponse(context, created);
                    });

                    endpoints.MapPut("/api/v1/services/{id}", async context =>
                    {
                        var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString()!);
                        var dto = await context.Request.ReadFromJsonAsync<UpdateServiceDto>();
                        var updated = new ServiceDto
                        {
                            Id = id,
                            Name = dto?.Name ?? "Updated Service",
                            Description = dto?.Description ?? "Updated Description",
                            BeginDate = dto?.BeginDate ?? DateTimeOffset.UtcNow,
                            EndDate = dto?.EndDate,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = "test-user",
                            UpdatedAt = DateTimeOffset.UtcNow,
                            UpdatedBy = "test-user"
                        };
                        await WriteJsonResponse(context, updated);
                    });

                    endpoints.MapDelete("/api/v1/services/{id}", async context =>
                    {
                        context.Response.StatusCode = 204;
                        await Task.CompletedTask;
                    });

                    endpoints.MapGet("/api/v1/tariffplans", async context =>
                    {
                        var tariffs = CreateTariffPlanDtoList(3);
                        await WriteJsonResponse(context, tariffs);
                    });

                    endpoints.MapGet("/api/v1/tariffplans/{id}", async context =>
                    {
                        var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString()!);
                        var tariff = CreateTariffPlanDto(id);
                        await WriteJsonResponse(context, tariff);
                    });

                    endpoints.MapGet("/api/v1/tariffplans/by-service/{serviceId}", async context =>
                    {
                        var serviceId = Guid.Parse(context.Request.RouteValues["serviceId"]?.ToString()!);
                        var tariffs = CreateTariffPlanDtoList(2, serviceId);
                        await WriteJsonResponse(context, tariffs);
                    });

                    endpoints.MapGet("/api/v1/tariffplans/by-service/active/{serviceId}", async context =>
                    {
                        var tariffs = CreateTariffPlanShortDataDtoList(2);
                        await WriteJsonResponse(context, tariffs);
                    });

                    endpoints.MapPost("/api/v1/tariffplans", async context =>
                    {
                        var dto = await context.Request.ReadFromJsonAsync<CreateTariffPlanDto>();
                        var created = new TariffPlanDto
                        {
                            Id = Guid.NewGuid(),
                            Name = dto?.Name ?? "Default Tariff",
                            Description = dto?.Description ?? "Default Description",
                            Price = dto?.Price ?? 100,
                            ServiceId = dto?.ServiceId ?? Guid.NewGuid(),
                            BeginDate = dto?.BeginDate ?? DateTimeOffset.UtcNow,
                            EndDate = dto?.EndDate
                        };
                        context.Response.StatusCode = 201;
                        await WriteJsonResponse(context, created);
                    });

                    endpoints.MapPut("/api/v1/tariffplans/{id}", async context =>
                    {
                        var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString()!);
                        var dto = await context.Request.ReadFromJsonAsync<UpdateTariffPlanDto>();
                        var updated = new TariffPlanDto
                        {
                            Id = id,
                            Name = dto?.Name ?? "Updated Tariff",
                            Description = dto?.Description ?? "Updated Description",
                            Price = dto?.Price ?? 100,
                            ServiceId = Guid.NewGuid(),
                            BeginDate = dto?.BeginDate ?? DateTimeOffset.UtcNow,
                            EndDate = dto?.EndDate
                        };
                        await WriteJsonResponse(context, updated);
                    });

                    endpoints.MapDelete("/api/v1/tariffplans/{id}", async context =>
                    {
                        context.Response.StatusCode = 204;
                        await Task.CompletedTask;
                    });
                });
            });

        _testServer = new TestServer(builder);
        _httpClient = _testServer.CreateClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5000/");
        _client = new ServicesDirectoryClient(_httpClient);
    }

    #region Helper Methods

    private static async Task WriteJsonResponse<T>(HttpContext context, T data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }

    private static ServiceDto CreateServiceDto(Guid? id = null, string? name = null)
    {
        return new AutoFaker<ServiceDto>()
            .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(dto => dto.Name, f => name ?? f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<ServiceDto> CreateServiceDtoList(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateServiceDto())
            .ToList();
    }

    private static ServiceShortDataDto CreateServiceShortDataDto()
    {
        return new AutoFaker<ServiceShortDataDto>()
            .RuleFor(dto => dto.Id, f => f.Random.Guid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<ServiceShortDataDto> CreateServiceShortDataDtoList(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateServiceShortDataDto())
            .ToList();
    }

    private static ServiceFullDataDto CreateServiceFullDataDto(Guid? id = null)
    {
        return new AutoFaker<ServiceFullDataDto>()
            .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, f => CreateTariffPlanDtoList(f.Random.Int(1, 3)))
            .Generate();
    }

    private static TariffPlanDto CreateTariffPlanDto(Guid? id = null, Guid? serviceId = null)
    {
        return new AutoFaker<TariffPlanDto>()
            .RuleFor(dto => dto.Id, _ => id ?? Guid.NewGuid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, _ => serviceId ?? Guid.NewGuid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.PastOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static List<TariffPlanDto> CreateTariffPlanDtoList(int count, Guid? serviceId = null)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateTariffPlanDto(serviceId: serviceId))
            .ToList();
    }

    private static TariffPlanShortDataDto CreateTariffPlanShortDataDto()
    {
        return new AutoFaker<TariffPlanShortDataDto>()
            .RuleFor(dto => dto.Id, f => f.Random.Guid())
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .Generate();
    }

    private static List<TariffPlanShortDataDto> CreateTariffPlanShortDataDtoList(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateTariffPlanShortDataDto())
            .ToList();
    }

    private static CreateServiceDto CreateCreateServiceDto()
    {
        return new AutoFaker<CreateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<CreateTariffPlanDto>())
            .Generate();
    }

    private static UpdateServiceDto CreateUpdateServiceDto()
    {
        return new AutoFaker<UpdateServiceDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .RuleFor(dto => dto.TariffPlans, _ => new List<UpdateTariffPlanFromServiceDto>())
            .Generate();
    }

    private static CreateTariffPlanDto CreateCreateTariffPlanDto()
    {
        return new AutoFaker<CreateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.ServiceId, f => f.Random.Guid())
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    private static UpdateTariffPlanDto CreateUpdateTariffPlanDto()
    {
        return new AutoFaker<UpdateTariffPlanDto>()
            .RuleFor(dto => dto.Name, f => f.Commerce.ProductName())
            .RuleFor(dto => dto.Description, f => f.Lorem.Sentence())
            .RuleFor(dto => dto.Price, f => f.Random.Decimal(1, 999))
            .RuleFor(dto => dto.BeginDate, f => f.Date.FutureOffset())
            .RuleFor(dto => dto.EndDate, _ => null)
            .Generate();
    }

    #endregion

    #region Services Tests

    [Fact]
    public async Task GetAllServicesAsync_WhenServicesExist_ReturnsServices()
    {
        // Act
        var result = await _client.GetAllServicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllServicesAsync_WithIncludes_ReturnsServices()
    {
        // Act
        var result = await _client.GetAllServicesAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetServiceByIdAsync_WhenServiceExists_ReturnsService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _client.GetServiceByIdAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
    }

    [Fact]
    public async Task GetFullServiceDataAsync_WhenServiceExists_ReturnsFullServiceData()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _client.GetFullServiceDataAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.TariffPlans.ShouldNotBeNull();
        result.TariffPlans.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetActiveServicesAsync_WhenActiveServicesExist_ReturnsActiveServices()
    {
        // Act
        var result = await _client.GetActiveServicesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateServiceAsync_WhenValidDto_ReturnsCreatedService()
    {
        // Arrange
        var createDto = CreateCreateServiceDto();

        // Act
        var result = await _client.CreateServiceAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.Description.ShouldBe(createDto.Description);
    }

    [Fact]
    public async Task UpdateServiceAsync_WhenValidDto_ReturnsUpdatedService()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var updateDto = CreateUpdateServiceDto();

        // Act
        var result = await _client.UpdateServiceAsync(serviceId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceId);
        result.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task DeleteServiceAsync_WhenServiceExists_DoesNotThrow()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var exception = await Record.ExceptionAsync(() => _client.DeleteServiceAsync(serviceId));

        // Assert
        exception.ShouldBeNull();
    }

    #endregion

    #region TariffPlans Tests

    [Fact]
    public async Task GetAllTariffPlansAsync_WhenTariffsExist_ReturnsTariffPlans()
    {
        // Act
        var result = await _client.GetAllTariffPlansAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllTariffPlansAsync_WithIncludes_ReturnsTariffPlans()
    {
        // Act
        var result = await _client.GetAllTariffPlansAsync(true);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetTariffPlanByIdAsync_WhenTariffExists_ReturnsTariffPlan()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        // Act
        var result = await _client.GetTariffPlanByIdAsync(tariffId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffId);
    }

    [Fact]
    public async Task GetTariffPlansByServiceAsync_WhenTariffsExist_ReturnsTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _client.GetTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        foreach (var tariff in result)
        {
            tariff.ServiceId.ShouldBe(serviceId);
        }
    }

    [Fact]
    public async Task GetActiveTariffPlansByServiceAsync_WhenActiveTariffsExist_ReturnsActiveTariffs()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        // Act
        var result = await _client.GetActiveTariffPlansByServiceAsync(serviceId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateTariffPlanAsync_WhenValidDto_ReturnsCreatedTariffPlan()
    {
        // Arrange
        var createDto = CreateCreateTariffPlanDto();

        // Act
        var result = await _client.CreateTariffPlanAsync(createDto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(createDto.Name);
        result.ServiceId.ShouldBe(createDto.ServiceId);
        result.Price.ShouldBe(createDto.Price);
    }

    [Fact]
    public async Task UpdateTariffPlanAsync_WhenValidDto_ReturnsUpdatedTariffPlan()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var updateDto = CreateUpdateTariffPlanDto();

        // Act
        var result = await _client.UpdateTariffPlanAsync(tariffId, updateDto);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(tariffId);
        result.Name.ShouldBe(updateDto.Name);
    }

    [Fact]
    public async Task DeleteTariffPlanAsync_WhenTariffExists_DoesNotThrow()
    {
        // Arrange
        var tariffId = Guid.NewGuid();

        // Act
        var exception = await Record.ExceptionAsync(() => _client.DeleteTariffPlanAsync(tariffId));

        // Assert
        exception.ShouldBeNull();
    }

    #endregion

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        _testServer.Dispose();
        await Task.CompletedTask;
    }
}