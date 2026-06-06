FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DamMonitor.sln", "."]
COPY ["DamMonitor.Domain/DamMonitor.Domain.csproj", "DamMonitor.Domain/"]
COPY ["DamMonitor.Infrastructure/DamMonitor.Infrastructure.csproj", "DamMonitor.Infrastructure/"]
COPY ["DamMonitor.Host/DamMonitor.Host.csproj", "DamMonitor.Host/"]
RUN dotnet restore "DamMonitor.sln"
COPY . .
RUN dotnet publish "DamMonitor.Host/DamMonitor.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DamMonitor.Host.dll"]
