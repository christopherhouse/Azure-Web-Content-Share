# Build stage
FROM node:24-alpine AS build
WORKDIR /app

# Copy package files
COPY src/Frontend/package*.json ./
RUN npm ci --only=production

# Copy source code and build
COPY src/Frontend/ ./
RUN npm run build

# Production stage with nginx
FROM nginx:1.25-alpine AS production

# The nginx Alpine image already has nginx user/group, so we just need to configure permissions
# Copy built application
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY src/Frontend/nginx.conf /etc/nginx/nginx.conf

# Create required directories and set permissions
RUN mkdir -p /var/cache/nginx /var/log/nginx && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# Switch to non-root user
USER nginx

# Expose port 8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

CMD ["nginx", "-g", "daemon off;"]