# Build stage
FROM node:24-alpine AS build
WORKDIR /app

# Copy package files
COPY src/Frontend/package*.json ./
RUN npm ci --omit=dev

# Copy source code and build
COPY src/Frontend/ ./
RUN npm run build

# Production stage with Caddy
FROM caddy:2.8 AS production

# Copy built application
COPY --from=build /app/dist /usr/share/caddy

# Copy Caddy configuration
COPY src/Frontend/Caddyfile /etc/caddy/Caddyfile

# Create caddy log directory
RUN mkdir -p /var/log/caddy

# Expose port 8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Caddy runs in the foreground by default
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile"]