# Seeds a development database with game data. Development only — this image is not deployed.
# Build from the REPOSITORY ROOT:
#
#   docker build -f src/FantasyCritic.LocalDatabaseTool/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FantasyCritic.LocalDatabaseTool/FantasyCritic.LocalDatabaseTool.csproj", "src/FantasyCritic.LocalDatabaseTool/"]
COPY ["src/FantasyCritic.Lib/FantasyCritic.Lib.csproj", "src/FantasyCritic.Lib/"]
COPY ["src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "src/FantasyCritic.MySQL/"]
RUN dotnet restore "src/FantasyCritic.LocalDatabaseTool/FantasyCritic.LocalDatabaseTool.csproj"
COPY . .
RUN dotnet build "src/FantasyCritic.LocalDatabaseTool/FantasyCritic.LocalDatabaseTool.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FantasyCritic.LocalDatabaseTool/FantasyCritic.LocalDatabaseTool.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FantasyCritic.LocalDatabaseTool.dll"]
