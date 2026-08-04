FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Contracts/ClientOpsPortal.Services.Notifications.Contracts.csproj ClientOpsPortal.Services.Notifications.Contracts/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Client/ClientOpsPortal.Services.Notifications.Client.csproj ClientOpsPortal.Services.Notifications.Client/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications/ClientOpsPortal.Services.Notifications.csproj ClientOpsPortal.Services.Notifications/

RUN dotnet restore ClientOpsPortal.Services.Notifications/ClientOpsPortal.Services.Notifications.csproj

COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Contracts/ ClientOpsPortal.Services.Notifications.Contracts/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications.Client/ ClientOpsPortal.Services.Notifications.Client/
COPY src/services/notifications/ClientOpsPortal.Services.Notifications/ ClientOpsPortal.Services.Notifications/

WORKDIR /app/ClientOpsPortal.Services.Notifications
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.Notifications.dll"]