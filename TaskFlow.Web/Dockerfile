# ============================
# 1. BUILD ANGULAR APP
# ============================
FROM node:20-alpine AS build
WORKDIR /app


# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci

# Copy all source code
COPY . .

# Accept API_BASE_URL as a build argument
ARG API_BASE_URL
# Overwrite environment.prod.ts with real API URL
RUN echo "export const environment = { production: true, apiBaseUrl: '$API_BASE_URL' };" > src/environments/environment.prod.ts

# Build production Angular app
RUN npm run build -- --configuration production


# ============================
# 2. SERVE WITH NGINX
# ============================
FROM nginx:stable-alpine AS final

# Copy Angular build output
COPY --from=build /app/dist/TaskFlow.Web/browser /usr/share/nginx/html

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
