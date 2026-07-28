using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Domain.Models.Reports;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Infrastructure.Data.Repositories;

public class ReportsRepository : IReportsRepository
{
    private readonly ClientOpsPortalDbContext _context;

    public ReportsRepository(ClientOpsPortalDbContext context) => _context = context;

    public async Task<IEnumerable<ServiceStatusReadModel>> GetServicesWithStatsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Services
            .AsNoTracking()
            .Select(s => new ServiceStatusReadModel(
                ServiceId: s.Id,
                ServiceName: s.Name,
                ServiceDescription: s.Description,
                BeginDate: s.BeginDate,
                EndDate: s.EndDate,
                TotalSubscriptions: _context.Subscriptions.Count(sub => sub.ServiceId == s.Id),
                ActiveSubscriptions: _context.Subscriptions.Count(sub => sub.ServiceId == s.Id && (sub.EndDate == null || sub.EndDate > now)),
                InactiveSubscriptions: _context.Subscriptions.Count(sub => sub.ServiceId == s.Id && sub.EndDate != null && sub.EndDate <= now),
                AverageTariffPrice: _context.TariffPlans.Any(tp => tp.ServiceId == s.Id)
                    ? _context.TariffPlans.Where(tp => tp.ServiceId == s.Id).Average(tp => tp.Price)
                    : null
            ))
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<Subscription> Items, int TotalCount)> GetActiveSubscriptionsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.EndDate == null || s.EndDate > now)
            .Include(s => s.Contract!).ThenInclude(c => c.Abonent)
            .Include(s => s.Service)
            .Include(s => s.TariffPlan)
            .OrderBy(s => s.BeginDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsForDynamicsAsync(
        DateTimeOffset dateFrom, DateTimeOffset dateTo, Guid? serviceId, Guid? tariffPlanId,
        CancellationToken ct = default)
    {
        var query = _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Service)
            .Include(s => s.TariffPlan)
            .Include(s => s.Contract!).ThenInclude(c => c.Abonent)
            .AsQueryable();

        if (serviceId.HasValue) query = query.Where(s => s.ServiceId == serviceId.Value);
        if (tariffPlanId.HasValue) query = query.Where(s => s.TariffPlanId == tariffPlanId.Value);

        return await query.ToListAsync(ct);
    }
}