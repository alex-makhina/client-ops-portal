FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/services/auth/ClientOpsPortal.Services.Auth.Contracts/ClientOpsPortal.Services.Auth.Contracts.csproj ClientOpsPortal.Services.Auth.Contracts/
COPY src/services/auth/ClientOpsPortal.Services.Auth/ClientOpsPortal.Services.Auth.csproj ClientOpsPortal.Services.Auth/

RUN dotnet restore ClientOpsPortal.Services.Auth/ClientOpsPortal.Services.Auth.csproj

COPY src/services/auth/ClientOpsPortal.Services.Auth.Contracts/ ClientOpsPortal.Services.Auth.Contracts/
COPY src/services/auth/ClientOpsPortal.Services.Auth/ ClientOpsPortal.Services.Auth/

WORKDIR /app/ClientOpsPortal.Services.Auth
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.Auth.dll"]