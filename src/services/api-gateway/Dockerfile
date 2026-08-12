FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY src/services/api-gateway/ClientOpsPortal.ApiGateway/ClientOpsPortal.ApiGateway.csproj .
RUN dotnet restore ClientOpsPortal.ApiGateway.csproj
COPY src/services/api-gateway/ClientOpsPortal.ApiGateway/ .
RUN dotnet publish ClientOpsPortal.ApiGateway.csproj -c Release -o /out

FROM base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ClientOpsPortal.ApiGateway.dll"]