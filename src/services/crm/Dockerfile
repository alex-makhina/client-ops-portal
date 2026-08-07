# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files
COPY src/services/crm/ClientOpsPortal.Api/ClientOpsPortal.Api.csproj ClientOpsPortal.Api/
COPY src/services/crm/ClientOpsPortal.Application/ClientOpsPortal.Application.csproj ClientOpsPortal.Application/
COPY src/services/crm/ClientOpsPortal.Domain/ClientOpsPortal.Domain.csproj ClientOpsPortal.Domain/
COPY src/services/crm/ClientOpsPortal.Infrastructure/ClientOpsPortal.Infrastructure.csproj ClientOpsPortal.Infrastructure/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Client/ClientOpsPortal.Services.Directory.Client.csproj ../../services-directory/ClientOpsPortal.Services.Directory.Client/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ClientOpsPortal.Services.Directory.Contracts.csproj ../../services-directory/ClientOpsPortal.Services.Directory.Contracts/
COPY src/services/auth/ClientOpsPortal.Services.Auth.Client/ClientOpsPortal.Services.Auth.Client.csproj ../../auth/ClientOpsPortal.Services.Auth.Client/
COPY src/services/auth/ClientOpsPortal.Services.Auth.Contracts/ClientOpsPortal.Services.Auth.Contracts.csproj ../../auth/ClientOpsPortal.Services.Auth.Contracts/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Client/ClientOpsPortal.Services.Notifications.Client.csproj ../../notifications/ClientOpsPortal.Services.Notifications.Client/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Contracts/ClientOpsPortal.Services.Notifications.Contracts.csproj ../../notifications/ClientOpsPortal.Services.Notifications.Contracts/

RUN dotnet restore ClientOpsPortal.Api/ClientOpsPortal.Api.csproj

# Copy source code
COPY src/services/crm/ClientOpsPortal.Api/ ClientOpsPortal.Api/
COPY src/services/crm/ClientOpsPortal.Application/ ClientOpsPortal.Application/
COPY src/services/crm/ClientOpsPortal.Domain/ ClientOpsPortal.Domain/
COPY src/services/crm/ClientOpsPortal.Infrastructure/ ClientOpsPortal.Infrastructure/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Client/ ../../services-directory/ClientOpsPortal.Services.Directory.Client/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ ../../services-directory/ClientOpsPortal.Services.Directory.Contracts/
COPY src/services/auth/ClientOpsPortal.Services.Auth.Client/ ../../auth/ClientOpsPortal.Services.Auth.Client/
COPY src/services/auth/ClientOpsPortal.Services.Auth.Contracts/ ../../auth/ClientOpsPortal.Services.Auth.Contracts/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Client/ ../../notifications/ClientOpsPortal.Services.Notifications.Client/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Contracts/ ../../notifications/ClientOpsPortal.Services.Notifications.Contracts/

WORKDIR /app/ClientOpsPortal.Api
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Api.dll"]