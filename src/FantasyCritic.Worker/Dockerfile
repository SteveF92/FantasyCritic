# The job worker (Phase 4b): scheduler, runner and canceller. Build from the REPOSITORY ROOT:
#
#   docker build -f src/FantasyCritic.Worker/Dockerfile .
#
# The aspnet image, not runtime: Hosting and EmailTemplates reference the ASP.NET Core shared framework.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FantasyCritic.Worker/FantasyCritic.Worker.csproj", "src/FantasyCritic.Worker/"]
COPY ["src/FantasyCritic.AWS/FantasyCritic.AWS.csproj", "src/FantasyCritic.AWS/"]
COPY ["src/FantasyCritic.EmailTemplates/FantasyCritic.EmailTemplates.csproj", "src/FantasyCritic.EmailTemplates/"]
COPY ["src/FantasyCritic.Hosting/FantasyCritic.Hosting.csproj", "src/FantasyCritic.Hosting/"]
COPY ["src/FantasyCritic.Lib/FantasyCritic.Lib.csproj", "src/FantasyCritic.Lib/"]
COPY ["src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "src/FantasyCritic.MySQL/"]
COPY ["src/FantasyCritic.Postmark/FantasyCritic.Postmark.csproj", "src/FantasyCritic.Postmark/"]
RUN dotnet restore "src/FantasyCritic.Worker/FantasyCritic.Worker.csproj"
COPY . .
RUN dotnet build "src/FantasyCritic.Worker/FantasyCritic.Worker.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FantasyCritic.Worker/FantasyCritic.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /var/log/fantasy-critic/worker && chown -R $APP_UID /var/log/fantasy-critic
USER $APP_UID

ENTRYPOINT ["dotnet", "FantasyCritic.Worker.dll"]
