# API Gateway

This directory contains the API Gateway component built with Caddy 2.8 and security plugins. The API Gateway provides:

## 🛡️ Security Features

- **ModSecurity v3-like WAF protection** using Caddy Security plugin
- **Rate limiting** with different zones for various endpoints
- **Security headers** including CSP, HSTS, and clickjacking protection
- **Attack pattern blocking** for SQL injection, XSS, path traversal, and command injection
- **Structured logging** with security event tracking

## 🔄 Reverse Proxy

The API Gateway acts as a reverse proxy to the internal API container, providing:
- Load balancing (currently single upstream)
- Health checks for upstream services
- Request header forwarding for proper client information
- Upstream health monitoring

## 📊 Rate Limiting

Different rate limiting zones are configured:
- **API Zone**: 100 requests per minute per IP for general API endpoints
- **Upload Zone**: 10 requests per minute per IP for file uploads
- **Auth Zone**: 20 requests per minute per IP for authentication endpoints

## 🏗️ Architecture

```
Internet → API Gateway (Caddy) → Internal API (ASP.NET Core)
                ↓
         Security & Rate Limiting
```

## 🚀 Deployment

The API Gateway is deployed as a Container App with:
- Public ingress (external traffic allowed)
- Minimum 1 replica for availability
- Auto-scaling based on HTTP requests
- Custom Caddy build with security plugins

## ⚙️ Configuration

Environment variables:
- `API_INTERNAL_URL`: Internal URL of the API container app
- `AZURE_CLIENT_ID`: Azure AD client ID (optional)
- `AZURE_CLIENT_SECRET`: Azure AD client secret (optional)
- `AZURE_TENANT_ID`: Azure AD tenant ID (optional)
- `JWT_SHARED_KEY`: Shared key for JWT validation (optional)

## 🔍 Monitoring

The API Gateway provides structured JSON logging with:
- Request/response details
- Security events
- Performance metrics
- Error tracking
- Health check status

## 🏃‍♂️ Local Development

To run locally:

```bash
# Copy environment variables
cp .env.example .env

# Build and run with Docker
docker build -t api-gateway .
docker run -p 8080:8080 --env-file .env api-gateway
```

Or with Caddy directly (requires custom build with plugins):

```bash
caddy run --config Caddyfile
```