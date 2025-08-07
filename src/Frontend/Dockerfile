# Build stage
FROM node:24-alpine AS build
WORKDIR /app

# Copy package files
COPY src/Frontend/package*.json ./
RUN npm ci --omit=dev

# Copy source code and build
COPY src/Frontend/ ./
RUN npm run build

# Production stage with Nginx
FROM nginx:1.26-alpine AS production

# Copy built application
COPY --from=build /app/dist /usr/share/nginx/html

# Copy Nginx configuration
COPY src/Frontend/nginx.conf /etc/nginx/nginx.conf

# Copy startup script for runtime configuration injection
COPY src/Frontend/startup.sh /usr/local/bin/startup.sh
RUN chmod +x /usr/local/bin/startup.sh

# Create nginx log directory  
RUN mkdir -p /var/log/nginx

# Expose port 8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Use startup script instead of direct Nginx
CMD ["/usr/local/bin/startup.sh"]