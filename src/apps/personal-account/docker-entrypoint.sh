#!/bin/sh
set -e

if [ -n "$VITE_API_URL" ]; then
    find /usr/share/nginx/html -type f -name "*.js" \
        -exec sed -i "s|http://localhost:5079/api/v1|$VITE_API_URL|g" {} \;
fi

exec "$@"
