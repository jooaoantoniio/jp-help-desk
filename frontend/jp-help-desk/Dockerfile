# Frontend do JP Help Desk — compila o Angular e serve os arquivos estáticos com nginx.
# Build (a partir da raiz do repositório):  docker build -t jphelpdesk-web frontend/jp-help-desk

# ---------- 1. Build: Node só para compilar ----------
FROM node:24-alpine AS build
WORKDIR /app

# Dependências primeiro (cache): "npm ci" instala exatamente o que está no package-lock.json.
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npx ng build --configuration production

# ---------- 2. Runtime: nginx sem root, escutando na porta 8080 ----------
FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/jp-help-desk/browser /usr/share/nginx/html

EXPOSE 8080
