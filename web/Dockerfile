# Imagem Debian (slim) e não Alpine no build: o package-lock.json foi gerado em glibc, e os
# binários nativos opcionais (esbuild, rollup) são resolvidos por plataforma.
FROM node:24-slim AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
# O build de produção baixa as Google Fonts para embutir no CSS: precisa de internet no build.
RUN npx ng build --configuration production


FROM nginx:stable-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 80
