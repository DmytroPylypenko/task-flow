# ============================
# 1. BUILD .NET API
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Allow passing environment as build-arg
ARG DOTNET_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=${DOTNET_ENVIRONMENT}

# Copy project and restore dependencies
COPY TaskFlow.Api/*.csproj ./ 
RUN dotnet restore

# Copy source code
COPY TaskFlow.Api/. .

# Publish optimized, trimmed output for production
RUN dotnet publish -c Release -o /app/publish

# ============================
# 2. RUNTIME IMAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Environment variables for containerized .NET app
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV DOTNET_NOLOGO=true

# Expose port
EXPOSE 8080

# Health check for Docker / Azure
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
  
# Run the API
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
