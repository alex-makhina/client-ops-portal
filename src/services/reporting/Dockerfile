FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Копируем .csproj файлы (сохраняя структуру src/services/...)
COPY src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/*.csproj ./src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/
COPY src/services/reporting/ClientOpsPortal.Services.Reporting/*.csproj ./src/services/reporting/ClientOpsPortal.Services.Reporting/
COPY src/services/crm/ClientOpsPortal.Contracts/*.csproj ./src/services/crm/ClientOpsPortal.Contracts/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/*.csproj ./src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/

# Восстанавливаем зависимости
RUN dotnet restore src/services/reporting/ClientOpsPortal.Services.Reporting/ClientOpsPortal.Services.Reporting.csproj

# Копируем только нужные проекты (сохраняя структуру)
COPY src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/ ./src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/
COPY src/services/reporting/ClientOpsPortal.Services.Reporting/ ./src/services/reporting/ClientOpsPortal.Services.Reporting/
COPY src/services/crm/ClientOpsPortal.Contracts/ ./src/services/crm/ClientOpsPortal.Contracts/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ ./src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/

# Публикуем
WORKDIR /app/src/services/reporting/ClientOpsPortal.Services.Reporting
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.Reporting.dll"]