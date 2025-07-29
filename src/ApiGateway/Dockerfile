# API Gateway Dockerfile - Use standard Caddy with custom configuration
FROM caddy:2.8

# Create caddy log directory
RUN mkdir -p /var/log/caddy

# Copy Caddy configuration (modified to work without plugins)
COPY src/ApiGateway/Caddyfile /etc/caddy/Caddyfile

# Expose port 8080 for the gateway
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Start Caddy with the configuration
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile", "--adapter", "caddyfile"]