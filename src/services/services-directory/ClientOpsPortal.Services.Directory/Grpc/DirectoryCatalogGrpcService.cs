using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Grpc;
using ClientOpsPortal.Services.Directory.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ClientOpsPortal.Services.Directory.Grpc
{
    public class DirectoryCatalogGrpcService : DirectoryCatalog.DirectoryCatalogBase
    {
        private readonly DirectoryService _directoryService;

        public DirectoryCatalogGrpcService(DirectoryService directoryService)
        {
            _directoryService = directoryService;
        }

        public override async Task<ServiceListResponse> GetServices(GetServicesRequest request, ServerCallContext context)
        {
            var services = await _directoryService.GetAllServicesAsync(request.WithIncludes, context.CancellationToken);
            var response = new ServiceListResponse();
            response.Items.AddRange(services.Select(ToProto));
            return response;
        }

        public override async Task<ServiceShortListResponse> GetActiveServices(Empty request, ServerCallContext context)
        {
            var services = await _directoryService.GetActiveServicesAsync(context.CancellationToken);
            var response = new ServiceShortListResponse();
            response.Items.AddRange(services.Select(ToShortProto));
            return response;
        }

        public override async Task<ServiceResponse> GetServiceById(GetServiceByIdRequest request, ServerCallContext context)
        {
            var service = await _directoryService.GetServiceByIdAsync(Guid.Parse(request.Id), request.WithIncludes, context.CancellationToken);
            if (service is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Услуга с ID {request.Id} не найдена"));

            return new ServiceResponse { Service = ToProto(service) };
        }

        public override async Task<ServiceFullDataResponse> GetFullServiceData(GetServiceByIdRequest request, ServerCallContext context)
        {
            var service = await _directoryService.GetFullServiceDataAsync(Guid.Parse(request.Id), context.CancellationToken);
            if (service is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Услуга с ID {request.Id} не найдена"));

            return new ServiceFullDataResponse { Data = ToFullDataProto(service) };
        }

        public override async Task<TariffPlanListResponse> GetTariffPlans(GetTariffPlansRequest request, ServerCallContext context)
        {
            var tariffs = await _directoryService.GetAllTariffPlansAsync(request.WithIncludes, context.CancellationToken);
            var response = new TariffPlanListResponse();
            response.Items.AddRange(tariffs.Select(ToProto));
            return response;
        }

        public override async Task<TariffPlanResponse> GetTariffPlanById(GetTariffPlanByIdRequest request, ServerCallContext context)
        {
            var tariff = await _directoryService.GetTariffPlanByIdAsync(Guid.Parse(request.Id), request.WithIncludes, context.CancellationToken);
            if (tariff is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Тарифный план с ID {request.Id} не найден"));

            return new TariffPlanResponse { TariffPlan = ToProto(tariff) };
        }

        public override async Task<TariffPlanListResponse> GetTariffPlansByService(GetTariffPlansByServiceRequest request, ServerCallContext context)
        {
            var tariffs = await _directoryService.GetTariffPlansByServiceAsync(Guid.Parse(request.ServiceId), context.CancellationToken);
            var response = new TariffPlanListResponse();
            response.Items.AddRange(tariffs.Select(ToProto));
            return response;
        }

        public override async Task<TariffPlanShortListResponse> GetActiveTariffPlansByService(GetTariffPlansByServiceRequest request, ServerCallContext context)
        {
            var tariffs = await _directoryService.GetActiveTariffPlansByServiceAsync(Guid.Parse(request.ServiceId), context.CancellationToken);
            var response = new TariffPlanShortListResponse();
            response.Items.AddRange(tariffs.Select(ToShortProto));
            return response;
        }

        private static Service ToProto(ServiceDto dto) => new()
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Description = dto.Description,
            BeginDate = Timestamp.FromDateTimeOffset(dto.BeginDate),
            EndDate = dto.EndDate is { } end ? Timestamp.FromDateTimeOffset(end) : null,
            CreatedAt = Timestamp.FromDateTimeOffset(dto.CreatedAt),
            CreatedBy = dto.CreatedBy ?? string.Empty,
            UpdatedAt = dto.UpdatedAt is { } updated ? Timestamp.FromDateTimeOffset(updated) : null,
            UpdatedBy = dto.UpdatedBy ?? string.Empty
        };

        private static ServiceShort ToShortProto(ServiceShortDataDto dto) => new()
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Description = dto.Description,
            BeginDate = Timestamp.FromDateTimeOffset(dto.BeginDate),
            EndDate = dto.EndDate is { } end ? Timestamp.FromDateTimeOffset(end) : null
        };

        private static ServiceFullData ToFullDataProto(ServiceFullDataDto dto)
        {
            var message = new ServiceFullData
            {
                Id = dto.Id.ToString(),
                Name = dto.Name,
                Description = dto.Description,
                BeginDate = Timestamp.FromDateTimeOffset(dto.BeginDate),
                EndDate = dto.EndDate is { } end ? Timestamp.FromDateTimeOffset(end) : null
            };
            message.TariffPlans.AddRange(dto.TariffPlans.Select(ToProto));
            return message;
        }

        private static TariffPlan ToProto(TariffPlanDto dto) => new()
        {
            Id = dto.Id.ToString(),
            ServiceId = dto.ServiceId.ToString(),
            Name = dto.Name,
            Description = dto.Description,
            Price = (double)dto.Price,
            BeginDate = Timestamp.FromDateTimeOffset(dto.BeginDate),
            EndDate = dto.EndDate is { } end ? Timestamp.FromDateTimeOffset(end) : null,
            CreatedAt = Timestamp.FromDateTimeOffset(dto.CreatedAt),
            CreatedBy = dto.CreatedBy ?? string.Empty,
            UpdatedAt = dto.UpdatedAt is { } updated ? Timestamp.FromDateTimeOffset(updated) : null,
            UpdatedBy = dto.UpdatedBy ?? string.Empty
        };

        private static TariffPlanShort ToShortProto(TariffPlanShortDataDto dto) => new()
        {
            Id = dto.Id.ToString(),
            Name = dto.Name,
            Price = (double)dto.Price
        };
    }
}
