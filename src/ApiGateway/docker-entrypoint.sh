#!/bin/sh

# Startup script for nginx API Gateway
# Handles environment variable substitution for API_INTERNAL_URL

# Default API URL for local development
DEFAULT_API_URL="http://api:8080"

# Use provided API_INTERNAL_URL or default
API_URL=${API_INTERNAL_URL:-$DEFAULT_API_URL}

# Create the nginx configuration with proper API URL
# Replace the placeholder in the nginx.conf template
sed "s|http://API_PLACEHOLDER|$API_URL|g" /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

echo "API Gateway starting with API URL: $API_URL"

# Start nginx in foreground
exec nginx -g "daemon off;"