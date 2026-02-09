FROM node:22-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration inventarios --base-href /inventarios/ --deploy-url /inventarios/ --output-path /app/dist/inventarios
RUN npm run build -- --configuration peluquerias --base-href /peluquerias/ --deploy-url /peluquerias/ --output-path /app/dist/peluquerias

FROM nginx:1.27-alpine AS runtime

COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist/inventarios/browser /usr/share/nginx/html/inventarios
COPY --from=builder /app/dist/peluquerias/browser /usr/share/nginx/html/peluquerias

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
