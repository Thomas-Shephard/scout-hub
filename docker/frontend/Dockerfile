FROM node:25-alpine AS build
WORKDIR /app
COPY src/ScoutHub.Client/package*.json ./
RUN npm ci --ignore-scripts
COPY src/ScoutHub.Client/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY docker/frontend/nginx.conf /etc/nginx/nginx.conf

# Give nginx user permissions to directories it needs to write to
RUN touch /tmp/nginx.pid && \
    chown -R nginx:nginx /tmp/nginx.pid /var/cache/nginx /var/log/nginx /etc/nginx/nginx.conf /usr/share/nginx/html

USER nginx
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
