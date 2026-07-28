FROM node:22-alpine AS build
WORKDIR /app

COPY src/apps/personal-account/clientopsportal.personalaccount/package.json src/apps/personal-account/clientopsportal.personalaccount/package-lock.json ./

RUN npm ci

COPY src/apps/personal-account/clientopsportal.personalaccount/ ./

ARG VITE_API_URL=http://localhost:5079/api/v1
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

FROM nginx:alpine

COPY src/apps/personal-account/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

COPY src/apps/personal-account/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
