FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/services/subscription-history/ClientOpsPortal.Services.SubscriptionHistory.Contracts/ClientOpsPortal.Services.SubscriptionHistory.Contracts.csproj ClientOpsPortal.Services.SubscriptionHistory.Contracts/
COPY src/services/subscription-history/ClientOpsPortal.Services.SubscriptionHistory/ClientOpsPortal.Services.SubscriptionHistory.csproj ClientOpsPortal.Services.SubscriptionHistory/

RUN dotnet restore ClientOpsPortal.Services.SubscriptionHistory/ClientOpsPortal.Services.SubscriptionHistory.csproj

COPY src/services/subscription-history/ClientOpsPortal.Services.SubscriptionHistory.Contracts/ ClientOpsPortal.Services.SubscriptionHistory.Contracts/
COPY src/services/subscription-history/ClientOpsPortal.Services.SubscriptionHistory/ ClientOpsPortal.Services.SubscriptionHistory/

WORKDIR /app/ClientOpsPortal.Services.SubscriptionHistory
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.SubscriptionHistory.dll"]
