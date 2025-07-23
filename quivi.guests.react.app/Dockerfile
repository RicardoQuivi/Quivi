# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM nginx:alpine AS base
EXPOSE 3012

# This stage is used to build the service project
FROM node:23-alpine AS build
ARG OAUTH_URL
ARG API_URL
ARG SIGNALR_URL
ARG GUESTS_URL
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
ENV VITE_AUTH_URL="${OAUTH_URL}"
ENV VITE_API_URL="${API_URL}"
ENV VITE_SIGNALR_URL="${SIGNALR_URL}"
ENV VITE_GUESTS_APP_URL="${GUESTS_URL}"
RUN npm run build

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
RUN adduser -D -H -u 1001 -s /sbin/nologin webuser
RUN mkdir -p /app/www
COPY --from=build /app/dist /app/www
COPY nginx.conf /etc/nginx/templates/default.conf.template
RUN chown -R webuser:webuser /app/www && \
    chmod -R 755 /app/www && \
    # Nginx needs to read and write to these directories
    chown -R webuser:webuser /var/cache/nginx && \
    chown -R webuser:webuser /var/log/nginx && \
    chown -R webuser:webuser /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R webuser:webuser /var/run/nginx.pid && \
    chmod -R 777 /etc/nginx/conf.d
ENV NGINX_ENVSUBST_TEMPLATE_DIR=/etc/nginx/templates
ENV NGINX_ENVSUBST_TEMPLATE_SUFFIX=.template
ENV NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx/conf.d
ENV PORT=3012
USER webuser
CMD ["nginx", "-g", "daemon off;"]