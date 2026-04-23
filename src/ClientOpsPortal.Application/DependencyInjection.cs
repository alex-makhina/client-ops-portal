using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAbonentService, AbonentService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ITariffPlanService, TariffPlanService>();
            services.AddScoped<ISubscriptionHistoryService, SubscriptionHistoryService>();
            services.AddScoped<ISubscriptionHistoryStepService, SubscriptionHistoryStepService>();
            services.AddScoped<IReportsService, ReportsService>();

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<INotificationService, EmailNotificationService>();

            return services;
        }

        public static IServiceCollection AddEmailSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            return services;
        }
    }
}