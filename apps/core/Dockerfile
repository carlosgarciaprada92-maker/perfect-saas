FROM node:22-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS runtime

COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist/perfect-core/browser /usr/share/nginx/html/core

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
