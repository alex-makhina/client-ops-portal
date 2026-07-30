using Microsoft.AspNetCore.Mvc;
using System.Text;
using ClientOpsPortal.Services.Reporting.Services;
using ClientOpsPortal.Services.Reporting.Contracts.DTOs;

namespace ClientOpsPortal.Services.Reporting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "text/csv")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportsService _reportsService;

        public ReportsController(ReportsService reportsService) => _reportsService = reportsService;

        [HttpGet("services-status")]
        public async Task<IActionResult> GetServicesStatus(
            [FromQuery] string format = "json",
            CancellationToken ct = default)
        {
            var data = await _reportsService.GetServicesStatusAsync(ct);
            return format.Equals("csv", StringComparison.OrdinalIgnoreCase)
                ? await ReturnCsvAsync(data, "services-status", ct)
                : Ok(data);
        }

        [HttpGet("active-subscriptions")]
        public async Task<IActionResult> GetActiveSubscriptions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string format = "json",
            CancellationToken ct = default)
        {
            var data = await _reportsService.GetActiveSubscriptionsAsync(page, pageSize, ct);
            return format.Equals("csv", StringComparison.OrdinalIgnoreCase)
                ? await ReturnCsvAsync(data.Items, "active-subscriptions", ct)
                : Ok(data);
        }

        [HttpGet("subscriptions-dynamics")]
        public async Task<IActionResult> GetSubscriptionsDynamics(
            [FromQuery] DynamicsReportFilterDto filter,
            [FromQuery] string format = "json",
            CancellationToken ct = default)
        {
            var data = await _reportsService.GetSubscriptionsDynamicsAsync(filter, ct);
            return format.Equals("csv", StringComparison.OrdinalIgnoreCase)
                ? await ReturnCsvAsync(new[] { data }, "subscriptions-dynamics", ct)
                : Ok(data);
        }

        private async Task<IActionResult> ReturnCsvAsync<T>(IEnumerable<T> data, string reportName, CancellationToken ct) where T : class
        {
            var csv = await _reportsService.ExportToCsvAsync(data, reportName, ct);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"{reportName}-{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }
    }
}
