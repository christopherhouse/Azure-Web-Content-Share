# API Gateway Dockerfile - Use nginx instead of Caddy
FROM nginx:1.26-alpine

# Create nginx log directory
RUN mkdir -p /var/log/nginx

# Copy nginx configuration template
COPY src/ApiGateway/nginx.conf /etc/nginx/nginx.conf.template

# Copy and make executable the startup script
COPY src/ApiGateway/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Expose port 8080 for the gateway
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Use custom entrypoint for environment variable substitution
ENTRYPOINT ["/docker-entrypoint.sh"]