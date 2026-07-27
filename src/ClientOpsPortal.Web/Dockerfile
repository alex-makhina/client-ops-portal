# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY src/ClientOpsPortal.Web/ClientOpsPortal.Web.csproj src/ClientOpsPortal.Web/
RUN dotnet restore src/ClientOpsPortal.Web/ClientOpsPortal.Web.csproj

COPY src/ClientOpsPortal.Web/ src/ClientOpsPortal.Web/

RUN dotnet publish src/ClientOpsPortal.Web/ClientOpsPortal.Web.csproj -c Release -o /out

# .NET 10 generates hashed filenames (blazor.webassembly.XXX.js, dotnet.XXX.js)
# but the loader hardcodes import("./blazor.webassembly.js") and import("./dotnet.js").
# Strip content hashes from entry-point JS files.
RUN cd /out/wwwroot/_framework && \
    for f in *.js; do \
        [ ! -f "$f" ] && continue; \
        base=$(echo "$f" | sed -E 's/\.[a-z0-9]{8,}\.js$/.js/'); \
        [ "$base" = "$f" ] && continue; \
        [ -f "$base" ] && continue; \
        cp "$f" "$base"; \
    done

# Runtime stage
FROM nginx:alpine

COPY src/ClientOpsPortal.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /out/wwwroot /usr/share/nginx/html

COPY src/ClientOpsPortal.Web/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
