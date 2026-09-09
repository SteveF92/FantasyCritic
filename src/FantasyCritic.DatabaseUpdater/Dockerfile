# The DbUp migrator, run as a one-off job before the web app starts. Build from the
# REPOSITORY ROOT:
#
#   docker build -f src/FantasyCritic.DatabaseUpdater/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj", "src/FantasyCritic.DatabaseUpdater/"]
COPY ["src/FantasyCritic.AWS/FantasyCritic.AWS.csproj", "src/FantasyCritic.AWS/"]
COPY ["src/FantasyCritic.Lib/FantasyCritic.Lib.csproj", "src/FantasyCritic.Lib/"]
COPY ["src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "src/FantasyCritic.MySQL/"]
RUN dotnet restore "src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj"
COPY . .
RUN dotnet build "src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
# Scripts/**/*.sql is copied to the output by the project file, so GetScriptsRoot() finds
# /app/Scripts. Compose can still bind-mount over it during development to pick up new SQL
# without rebuilding.
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /var/log/fantasy-critic/databaseupdater && chown -R $APP_UID /var/log/fantasy-critic
USER $APP_UID

ENTRYPOINT ["dotnet", "FantasyCritic.DatabaseUpdater.dll"]
