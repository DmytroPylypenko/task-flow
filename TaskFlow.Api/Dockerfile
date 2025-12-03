# ============================
# 1. BUILD .NET API
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY TaskFlow.Api/*.csproj ./ 
RUN dotnet restore

COPY TaskFlow.Api/. .
RUN dotnet publish -c Release -o /app/publish


# ============================
# 2. RUNTIME IMAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

CMD ["dotnet", "TaskFlow.Api.dll"]
