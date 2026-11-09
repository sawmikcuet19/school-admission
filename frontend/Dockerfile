# syntax=docker/dockerfile:1

FROM node:22-alpine

WORKDIR /app

RUN npm install -g pnpm@11.24.0

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .

# The Vite dev server reads this at startup, so it can be overridden at runtime.
ENV VITE_API_BASE_URL=http://localhost:8080 \
    CHOKIDAR_USEPOLLING=true

EXPOSE 5173

CMD ["pnpm", "dev", "--host", "0.0.0.0", "--port", "5173"]
