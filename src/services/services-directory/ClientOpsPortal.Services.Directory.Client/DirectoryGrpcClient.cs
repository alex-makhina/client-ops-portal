using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Grpc;

namespace ClientOpsPortal.Services.Directory.Client
{
    public class DirectoryGrpcClient : IDirectoryGrpcClient
    {
        private readonly DirectoryCatalog.DirectoryCatalogClient _client;

        public DirectoryGrpcClient(DirectoryCatalog.DirectoryCatalogClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var response = await _client.GetServicesAsync(new GetServicesRequest { WithIncludes = withIncludes }, cancellationToken: ct);
            return response.Items.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var response = await _client.GetTariffPlansAsync(new GetTariffPlansRequest { WithIncludes = withIncludes }, cancellationToken: ct);
            return response.Items.Select(ToDto).ToList();
        }

        private static ServiceDto ToDto(Service message) => new()
        {
            Id = Guid.Parse(message.Id),
            Name = message.Name,
            Description = message.Description,
            BeginDate = message.BeginDate.ToDateTimeOffset(),
            EndDate = message.EndDate?.ToDateTimeOffset(),
            CreatedAt = message.CreatedAt.ToDateTimeOffset(),
            CreatedBy = string.IsNullOrEmpty(message.CreatedBy) ? null : message.CreatedBy,
            UpdatedAt = message.UpdatedAt?.ToDateTimeOffset(),
            UpdatedBy = string.IsNullOrEmpty(message.UpdatedBy) ? null : message.UpdatedBy
        };

        private static TariffPlanDto ToDto(TariffPlan message) => new()
        {
            Id = Guid.Parse(message.Id),
            ServiceId = Guid.Parse(message.ServiceId),
            Name = message.Name,
            Description = message.Description,
            Price = (decimal)message.Price,
            BeginDate = message.BeginDate.ToDateTimeOffset(),
            EndDate = message.EndDate?.ToDateTimeOffset(),
            CreatedAt = message.CreatedAt.ToDateTimeOffset(),
            CreatedBy = string.IsNullOrEmpty(message.CreatedBy) ? null : message.CreatedBy,
            UpdatedAt = message.UpdatedAt?.ToDateTimeOffset(),
            UpdatedBy = string.IsNullOrEmpty(message.UpdatedBy) ? null : message.UpdatedBy
        };
    }
}
