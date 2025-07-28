# API Gateway Dockerfile with Caddy and rate limiting
FROM caddy:2.8-builder AS builder

# Install Caddy with rate limiting plugin
RUN xcaddy build \
    --with github.com/mholt/caddy-ratelimit

# Production stage
FROM alpine:3.19 AS production

# Install required packages for security and monitoring
RUN apk add --no-cache \
    ca-certificates \
    curl \
    tzdata \
    libcap \
    && rm -rf /var/cache/apk/*

# Copy the custom Caddy binary with security plugins
COPY --from=builder /usr/bin/caddy /usr/bin/caddy

# Create caddy user and directories
RUN adduser -D -s /bin/sh caddy && \
    mkdir -p /var/log/caddy /etc/caddy /var/lib/caddy && \
    chown -R caddy:caddy /var/log/caddy /etc/caddy /var/lib/caddy

# Copy Caddy configuration
COPY src/ApiGateway/Caddyfile /etc/caddy/Caddyfile

# Set capabilities to bind to privileged ports
RUN setcap 'cap_net_bind_service=+ep' /usr/bin/caddy

# Switch to non-root user
USER caddy

# Expose port 8080 for the gateway
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start Caddy
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile", "--adapter", "caddyfile"]