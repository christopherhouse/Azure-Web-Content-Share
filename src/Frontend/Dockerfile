# Build stage
FROM node:24-alpine AS build
WORKDIR /app

# Copy package files
COPY src/Frontend/package*.json ./
RUN npm ci --only=production

# Copy source code and build
COPY src/Frontend/ ./
RUN npm run build

# Caddy build stage with rate limiting plugin
FROM caddy:2.8-builder AS caddy-builder

# Install Caddy with rate limiting plugin
RUN xcaddy build \
    --with github.com/mholt/caddy-ratelimit

# Production stage with enhanced Caddy
FROM alpine:3.19 AS production

# Install required packages
RUN apk add --no-cache \
    ca-certificates \
    curl \
    tzdata \
    libcap \
    && rm -rf /var/cache/apk/*

# Copy the custom Caddy binary with security plugins
COPY --from=caddy-builder /usr/bin/caddy /usr/bin/caddy

# Copy built application
COPY --from=build /app/dist /usr/share/caddy

# Copy Caddy configuration
COPY src/Frontend/Caddyfile /etc/caddy/Caddyfile

# Create caddy user and set up directories
RUN adduser -D -s /bin/sh caddy && \
    mkdir -p /var/log/caddy /etc/caddy && \
    chown -R caddy:caddy /var/log/caddy /usr/share/caddy /etc/caddy

# Set capabilities to bind to privileged ports
RUN setcap 'cap_net_bind_service=+ep' /usr/bin/caddy

# Switch to non-root user
USER caddy

# Expose port 8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Caddy runs in the foreground by default
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile"]