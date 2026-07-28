# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/apps/admin-portal/ClientOpsPortal.Web/ClientOpsPortal.Web.csproj ClientOpsPortal.Web/
RUN dotnet restore ClientOpsPortal.Web/ClientOpsPortal.Web.csproj

COPY src/apps/admin-portal/ClientOpsPortal.Web/ ClientOpsPortal.Web/

RUN dotnet publish ClientOpsPortal.Web/ClientOpsPortal.Web.csproj -c Release -o /out

# Strip content hashes from entry-point JS files (.NET 10)
RUN cd /out/wwwroot/_framework && \
    for f in *.js; do \
        [ ! -f "$f" ] && continue; \
        base=$(echo "$f" | sed -E 's/\.[a-z0-9]{8,}\.js$/.js/'); \
        [ "$base" = "$f" ] && continue; \
        [ -f "$base" ] && continue; \
        cp "$f" "$base"; \
    done

FROM nginx:alpine

COPY src/apps/admin-portal/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /out/wwwroot /usr/share/nginx/html

COPY src/apps/admin-portal/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
