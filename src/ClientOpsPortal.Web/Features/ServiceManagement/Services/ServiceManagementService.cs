using ClientOpsPortal.Web.Features.ServiceManagement.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ClientOpsPortal.Web.Features.ServiceManagement.Services
{
    public class ServiceManagementService : IServiceManagementService
    {
        private readonly HttpClient _httpClient;

        public ServiceManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<List<ServiceListItem>> GetAllServicesAsync()
        {
            var response = await _httpClient.GetAsync("api/v1/Services/");
            if (!response.IsSuccessStatusCode)
                return new List<ServiceListItem>();

            return await response.Content.ReadFromJsonAsync<List<ServiceListItem>>() ?? new List<ServiceListItem>();
        }

        public async Task<ServiceFullItem?> GetFullServiceByIdAsync(Guid serviceId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Services/full/{serviceId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ServiceFullItem>();
        }

        public async Task<bool> CreateServiceAsync(CreateServiceRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Services", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/Services/{serviceId}", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteServiceAsync(Guid serviceId)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/Services/{serviceId}");
            return response.IsSuccessStatusCode;
        }
    }
}