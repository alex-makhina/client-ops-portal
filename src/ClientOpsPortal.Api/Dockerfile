# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/ClientOpsPortal.Api/ClientOpsPortal.Api.csproj src/ClientOpsPortal.Api/
COPY src/ClientOpsPortal.Application/ClientOpsPortal.Application.csproj src/ClientOpsPortal.Application/
COPY src/ClientOpsPortal.Domain/ClientOpsPortal.Domain.csproj src/ClientOpsPortal.Domain/
COPY src/ClientOpsPortal.Infrastructure/ClientOpsPortal.Infrastructure.csproj src/ClientOpsPortal.Infrastructure/

RUN dotnet restore src/ClientOpsPortal.Api/ClientOpsPortal.Api.csproj

COPY src/ClientOpsPortal.Api/ src/ClientOpsPortal.Api/
COPY src/ClientOpsPortal.Application/ src/ClientOpsPortal.Application/
COPY src/ClientOpsPortal.Domain/ src/ClientOpsPortal.Domain/
COPY src/ClientOpsPortal.Infrastructure/ src/ClientOpsPortal.Infrastructure/

WORKDIR /app/src/ClientOpsPortal.Api
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /out .

ENTRYPOINT ["dotnet", "ClientOpsPortal.Api.dll"]
