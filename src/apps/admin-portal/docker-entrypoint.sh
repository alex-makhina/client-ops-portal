#!/bin/sh
set -e

if [ -n "$API_BASE_URL" ]; then
    sed -i "s|\"ApiBaseUrl\": \"[^\"]*\"|\"ApiBaseUrl\": \"$API_BASE_URL\"|" /usr/share/nginx/html/appsettings.json
fi

if [ -n "$OIDC_AUTHORITY" ]; then
    sed -i "s|\"Authority\": \"[^\"]*\"|\"Authority\": \"$OIDC_AUTHORITY\"|" /usr/share/nginx/html/appsettings.json
fi

if [ -n "$OIDC_REDIRECT_URI" ]; then
    sed -i "s|\"RedirectUri\": \"[^\"]*\"|\"RedirectUri\": \"$OIDC_REDIRECT_URI\"|" /usr/share/nginx/html/appsettings.json
fi

if [ -n "$OIDC_POST_LOGOUT_REDIRECT_URI" ]; then
    sed -i "s|\"PostLogoutRedirectUri\": \"[^\"]*\"|\"PostLogoutRedirectUri\": \"$OIDC_POST_LOGOUT_REDIRECT_URI\"|" /usr/share/nginx/html/appsettings.json
fi

exec "$@"
