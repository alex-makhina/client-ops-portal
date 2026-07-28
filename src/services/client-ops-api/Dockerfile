# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files
COPY src/services/client-ops-api/ClientOpsPortal.Api/ClientOpsPortal.Api.csproj ClientOpsPortal.Api/
COPY src/services/client-ops-api/ClientOpsPortal.Application/ClientOpsPortal.Application.csproj ClientOpsPortal.Application/
COPY src/services/client-ops-api/ClientOpsPortal.Domain/ClientOpsPortal.Domain.csproj ClientOpsPortal.Domain/
COPY src/services/client-ops-api/ClientOpsPortal.Infrastructure/ClientOpsPortal.Infrastructure.csproj ClientOpsPortal.Infrastructure/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Client/ClientOpsPortal.Services.Directory.Client.csproj ../../services-directory/ClientOpsPortal.Services.Directory.Client/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ClientOpsPortal.Services.Directory.Contracts.csproj ../../services-directory/ClientOpsPortal.Services.Directory.Contracts/

RUN dotnet restore ClientOpsPortal.Api/ClientOpsPortal.Api.csproj

# Copy source code
COPY src/services/client-ops-api/ClientOpsPortal.Api/ ClientOpsPortal.Api/
COPY src/services/client-ops-api/ClientOpsPortal.Application/ ClientOpsPortal.Application/
COPY src/services/client-ops-api/ClientOpsPortal.Domain/ ClientOpsPortal.Domain/
COPY src/services/client-ops-api/ClientOpsPortal.Infrastructure/ ClientOpsPortal.Infrastructure/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Client/ ../../services-directory/ClientOpsPortal.Services.Directory.Client/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ ../../services-directory/ClientOpsPortal.Services.Directory.Contracts/

WORKDIR /app/ClientOpsPortal.Api
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Api.dll"]

