# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Jobs/AzureWebContentShare.Jobs.csproj", "Jobs/"]
COPY ["src/Api/AzureWebContentShare.Api.csproj", "Api/"]

# Restore dependencies
RUN dotnet restore "Jobs/AzureWebContentShare.Jobs.csproj"

# Copy source code
COPY src/ .

# Build the application
WORKDIR "/src/Jobs"
RUN dotnet build "AzureWebContentShare.Jobs.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AzureWebContentShare.Jobs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --uid 1000 appuser && \
    chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=publish --chown=appuser:appuser /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "AzureWebContentShare.Jobs.dll"]