using ClientOpsPortal.Web.Features.Reports.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Web.Features.Reports.Services
{
    public class ReportService : IReportService
    {
        private readonly HttpClient _httpClient;

        public ReportService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<List<ServiceStatusReportItem>> GetServicesStatusAsync(string format = "json")
        {
            var response = await _httpClient.GetAsync($"api/Reports/services-status?format={format}");
            if (!response.IsSuccessStatusCode)
                return new List<ServiceStatusReportItem>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ServiceStatusReportItem>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ServiceStatusReportItem>();
        }

        public async Task<ReportPaginationResult<ActiveSubscriptionReportItem>> GetActiveSubscriptionsAsync(
            int page = 1, int pageSize = 50, string format = "json")
        {
            var response = await _httpClient.GetAsync($"api/Reports/active-subscriptions?page={page}&pageSize={pageSize}&format={format}");
            if (!response.IsSuccessStatusCode)
                return new ReportPaginationResult<ActiveSubscriptionReportItem>();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReportPaginationResult<ActiveSubscriptionReportItem>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ReportPaginationResult<ActiveSubscriptionReportItem>();
        }

        public async Task<SubscriptionDynamicsReportItem> GetSubscriptionsDynamicsAsync(
            ReportFilterModel filter, string format = "json")
        {
            var queryParams = new List<string>();
            if (filter.DateFrom.HasValue)
                queryParams.Add($"DateFrom={filter.DateFrom.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
            if (filter.DateTo.HasValue)
                queryParams.Add($"DateTo={filter.DateTo.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
            if (filter.ServiceId.HasValue)
                queryParams.Add($"ServiceId={filter.ServiceId.Value}");
            if (filter.TariffPlanId.HasValue)
                queryParams.Add($"TariffPlanId={filter.TariffPlanId.Value}");
            if (!string.IsNullOrEmpty(filter.GroupBy))
                queryParams.Add($"GroupBy={filter.GroupBy}");
            queryParams.Add($"format={format}");

            var query = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"api/Reports/subscriptions-dynamics?{query}");

            if (!response.IsSuccessStatusCode)
                return new SubscriptionDynamicsReportItem();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SubscriptionDynamicsReportItem>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new SubscriptionDynamicsReportItem();
        }

        public async Task<byte[]> ExportReportAsync(string reportType, ReportFilterModel? filter = null)
        {
            var url = $"api/Reports/{reportType}?format=csv";
            if (filter != null)
            {
                var queryParams = new List<string>();
                if (filter.DateFrom.HasValue)
                    queryParams.Add($"DateFrom={filter.DateFrom.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
                if (filter.DateTo.HasValue)
                    queryParams.Add($"DateTo={filter.DateTo.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
                if (filter.ServiceId.HasValue)
                    queryParams.Add($"ServiceId={filter.ServiceId.Value}");
                if (filter.TariffPlanId.HasValue)
                    queryParams.Add($"TariffPlanId={filter.TariffPlanId.Value}");
                if (!string.IsNullOrEmpty(filter.GroupBy))
                    queryParams.Add($"GroupBy={filter.GroupBy}");

                if (queryParams.Any())
                    url += "&" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Array.Empty<byte>();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}