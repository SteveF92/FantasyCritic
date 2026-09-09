# The Discord command gateway (Phase 4a). Build from the REPOSITORY ROOT:
#
#   docker build -f src/FantasyCritic.DiscordBot/Dockerfile .
#
# This process handles slash commands. The web app keeps DiscordPushService and its own
# gateway connection on the same token, so exactly one container must run this one.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FantasyCritic.DiscordBot/FantasyCritic.DiscordBot.csproj", "src/FantasyCritic.DiscordBot/"]
COPY ["src/FantasyCritic.AWS/FantasyCritic.AWS.csproj", "src/FantasyCritic.AWS/"]
COPY ["src/FantasyCritic.Hosting/FantasyCritic.Hosting.csproj", "src/FantasyCritic.Hosting/"]
COPY ["src/FantasyCritic.Lib/FantasyCritic.Lib.csproj", "src/FantasyCritic.Lib/"]
COPY ["src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "src/FantasyCritic.MySQL/"]
RUN dotnet restore "src/FantasyCritic.DiscordBot/FantasyCritic.DiscordBot.csproj"
COPY . .
RUN dotnet build "src/FantasyCritic.DiscordBot/FantasyCritic.DiscordBot.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FantasyCritic.DiscordBot/FantasyCritic.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /var/log/fantasy-critic/discordbot && chown -R $APP_UID /var/log/fantasy-critic
USER $APP_UID

ENTRYPOINT ["dotnet", "FantasyCritic.DiscordBot.dll"]
