FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ClientOpsPortal.Services.Directory.Contracts.csproj ClientOpsPortal.Services.Directory.Contracts/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory/ClientOpsPortal.Services.Directory.csproj ClientOpsPortal.Services.Directory/

RUN dotnet restore ClientOpsPortal.Services.Directory/ClientOpsPortal.Services.Directory.csproj

COPY src/services/services-directory/ClientOpsPortal.Services.Directory.Contracts/ ClientOpsPortal.Services.Directory.Contracts/
COPY src/services/services-directory/ClientOpsPortal.Services.Directory/ ClientOpsPortal.Services.Directory/

WORKDIR /app/ClientOpsPortal.Services.Directory
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Services.Directory.dll"]
