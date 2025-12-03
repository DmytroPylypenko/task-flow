# ============================
# 1. BUILD ANGULAR APP
# ============================
FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production


# ============================
# 2. SERVE WITH NGINX
# ============================
FROM nginx:stable-alpine AS final

COPY --from=build /app/dist/TaskFlow.Web/browser /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
