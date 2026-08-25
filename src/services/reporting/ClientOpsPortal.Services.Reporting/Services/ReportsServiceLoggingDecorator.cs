using ClientOpsPortal.Services.Reporting.Contracts.DTOs;

namespace ClientOpsPortal.Services.Reporting.Services;

public class ReportsServiceLoggingDecorator : IReportsService
{
    private readonly IReportsService _inner;
    private readonly ILogger<ReportsServiceLoggingDecorator> _logger;

    public ReportsServiceLoggingDecorator(IReportsService inner, ILogger<ReportsServiceLoggingDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceStatusReportDto>> GetServicesStatusAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("[DECORATOR] Запрос списка статусов услуг");
        return await _inner.GetServicesStatusAsync(ct);
    }

    public async Task<ReportPaginationDto<ActiveSubscriptionReportDto>> GetActiveSubscriptionsAsync(
        int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        _logger.LogInformation("[DECORATOR] Запрос активных подписок (страница {Page}, размер {PageSize})", page, pageSize);

        var result = await _inner.GetActiveSubscriptionsAsync(page, pageSize, ct);

        _logger.LogInformation("[DECORATOR] Успешно получено {Count} записей активных подписок", result.Items.Count());
        return result;
    }

    public async Task<SubscriptionDynamicsReportDto> GetSubscriptionsDynamicsAsync(
        DynamicsReportFilterDto filter, CancellationToken ct = default)
    {
        _logger.LogInformation("[DECORATOR] Запрос динамики подписок с {From} по {To}", filter.DateFrom, filter.DateTo);
        return await _inner.GetSubscriptionsDynamicsAsync(filter, ct);
    }

    public async Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string reportName, CancellationToken ct = default) where T : class
    {
        _logger.LogInformation("[DECORATOR] Начало экспорта отчета '{ReportName}' в CSV. Записей: {Count}", reportName, data.Count());

        var result = await _inner.ExportToCsvAsync(data, reportName, ct);

        _logger.LogInformation("[DECORATOR] Экспорт отчета '{ReportName}' успешно завершен", reportName);
        return result;
    }
}