FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/ClientOpsPortal.Services.Reporting.Contracts.csproj ClientOpsPortal.Services.Reporting.Contracts/
COPY src/services/reporting/ClientOpsPortal.Services.Reporting/ClientOpsPortal.Services.Reporting.csproj ClientOpsPortal.Services.Reporting/

RUN dotnet restore ClientOpsPortal.Services.Reporting/ClientOpsPortal.Services.Reporting.csproj

COPY src/services/reporting/ClientOpsPortal.Services.Reporting.Contracts/ ClientOpsPortal.Services.Reporting.Contracts/
COPY src/services/reporting/ClientOpsPortal.Services.Reporting/ ClientOpsPortal.Services.Reporting/

WORKDIR /app/ClientOpsPortal.Services.Reporting
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.Reporting.dll"]
