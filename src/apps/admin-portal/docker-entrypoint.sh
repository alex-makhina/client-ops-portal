#!/bin/sh
set -e

if [ -n "$API_BASE_URL" ]; then
    sed -i "s|\"ApiBaseUrl\": \"[^\"]*\"|\"ApiBaseUrl\": \"$API_BASE_URL\"|" /usr/share/nginx/html/appsettings.json
fi

exec "$@"
