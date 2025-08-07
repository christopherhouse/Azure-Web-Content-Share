#!/bin/sh

# Runtime Environment Variable Injection Script
# This script creates a runtime configuration that gets injected into the built application

echo "🔧 Injecting runtime environment variables..."

# Create runtime config directory
mkdir -p /usr/share/nginx/html/config

# Create runtime configuration file
cat > /usr/share/nginx/html/config/runtime-config.js << EOF
window.__RUNTIME_CONFIG__ = {
  VITE_AZURE_CLIENT_ID: "${VITE_AZURE_CLIENT_ID}",
  VITE_AZURE_TENANT_ID: "${VITE_AZURE_TENANT_ID}",
  VITE_API_BASE_URL: "${VITE_API_BASE_URL}",
  VITE_API_CLIENT_ID: "${VITE_API_CLIENT_ID}",
  VITE_APPLICATIONINSIGHTS_CONNECTION_STRING: "${VITE_APPLICATIONINSIGHTS_CONNECTION_STRING}"
};
EOF

echo "✅ Runtime configuration injected"
echo "🔧 Runtime config contents:"
cat /usr/share/nginx/html/config/runtime-config.js

# Start Nginx
echo "🚀 Starting Nginx..."
exec nginx -g "daemon off;"