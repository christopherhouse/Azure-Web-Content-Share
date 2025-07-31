# API Gateway

This directory contains the API Gateway component built with nginx. The API Gateway provides:

## 🔄 Reverse Proxy

The API Gateway acts as a simple reverse proxy to the internal API container, providing:
- Request routing to `/api/*` endpoints
- Health check endpoint at `/health`
- Request header forwarding for proper client information
- 404 responses for unmatched routes

## 🏗️ Architecture

```
Internet → API Gateway (nginx) → Internal API (ASP.NET Core)
```

## 🚀 Deployment

The API Gateway is deployed as a Container App with:
- Public ingress (external traffic allowed)
- Minimum 1 replica for availability
- Auto-scaling based on HTTP requests
- nginx reverse proxy configuration

## ⚙️ Configuration

Environment variables:
- `API_INTERNAL_URL`: Internal URL of the API container app (defaults to `http://api:8080` for local development)

## 🔍 Monitoring

The API Gateway provides:
- Access and error logging via nginx
- Health check endpoint at `/health`
- Request forwarding to internal API

## 🏃‍♂️ Local Development

To run locally with Docker:

```bash
# Build and run the API Gateway
docker build -t api-gateway -f src/ApiGateway/Dockerfile .
docker run -p 8080:8080 -e API_INTERNAL_URL=http://host.docker.internal:8081 api-gateway
```

To run the entire solution locally:

```bash
# Run all containers with docker-compose
docker-compose up -d

# View logs
docker-compose logs -f api-gateway
```

## 📝 Configuration Details

The nginx configuration:
- Listens on port 8080
- `/health` endpoint returns "healthy" with 200 status
- `/api/*` routes forward to the internal API
- All other requests return 404
- Adds proper proxy headers for request forwarding