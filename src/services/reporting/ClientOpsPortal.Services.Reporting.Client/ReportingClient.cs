using ClientOpsPortal.Services.Reporting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ClientOpsPortal.Services.Reporting.Client
{
    public class ReportingClient(HttpClient http) : IReportingClient
    {
        private readonly HttpClient _http = http;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<IEnumerable<ServiceStatusReportDto>> GetServicesStatusReportAsync(
            string format = "json",
            CancellationToken ct = default)
            => await _http.GetFromJsonAsync<IEnumerable<ServiceStatusReportDto>>("api/v1/services-status", JsonOptions, ct) ?? [];

        public async Task<ReportPaginationDto<ActiveSubscriptionReportDto>?> GetActiveSubscriptionsReportAsync(
            int page = 1,
            int pageSize = 50,
            string format = "json",
            CancellationToken ct = default)
            => await _http.GetFromJsonAsync<ReportPaginationDto<ActiveSubscriptionReportDto>>("api/v1/active-subscriptions", JsonOptions, ct);

        public async Task<SubscriptionDynamicsReportDto?> GetSubscriptionsDynamicsReportAsync(
            DynamicsReportFilterDto filter, 
            string format = "json", 
            CancellationToken ct = default) 
            => await _http.GetFromJsonAsync<SubscriptionDynamicsReportDto>("api/v1/subscriptions-dynamics", JsonOptions, ct);
    }
}
