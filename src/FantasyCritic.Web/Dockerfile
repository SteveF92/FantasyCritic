# The web app. Build from the REPOSITORY ROOT, not from src/:
#
#   docker build -f src/FantasyCritic.Web/Dockerfile .
#
# The root is required because the NSwag tool manifest (.config/dotnet-tools.json) and the
# generation script (scripts/regenerate-api-client.sh) live above src/, and the Vue build
# cannot happen without them — see the client generation stage below.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Node.js + npm for the client app publish. FantasyCritic.Web references the client .esproj,
# whose publish step runs `npm run build` (vite) and lands the bundle in wwwroot.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# ------------------------------------------------------------------------------------------
# Restore, against project files alone so this layer caches across ordinary code changes.
# ------------------------------------------------------------------------------------------
COPY ["src/FantasyCritic.Web/FantasyCritic.Web.csproj", "src/FantasyCritic.Web/"]
COPY ["src/FantasyCritic.AWS/FantasyCritic.AWS.csproj", "src/FantasyCritic.AWS/"]
COPY ["src/FantasyCritic.EmailTemplates/FantasyCritic.EmailTemplates.csproj", "src/FantasyCritic.EmailTemplates/"]
COPY ["src/FantasyCritic.FakeRepo/FantasyCritic.FakeRepo.csproj", "src/FantasyCritic.FakeRepo/"]
COPY ["src/FantasyCritic.Hosting/FantasyCritic.Hosting.csproj", "src/FantasyCritic.Hosting/"]
COPY ["src/FantasyCritic.Lib/FantasyCritic.Lib.csproj", "src/FantasyCritic.Lib/"]
COPY ["src/FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "src/FantasyCritic.MySQL/"]
COPY ["src/FantasyCritic.Postmark/FantasyCritic.Postmark.csproj", "src/FantasyCritic.Postmark/"]
# FantasyCritic.Web references the client .esproj, and the JavaScript SDK reads
# package.json during evaluation to generate package.g.props. Without these the
# restore layer silently resolves nothing for that reference -- restore still exits 0
# and the implicit restore during "dotnet build" picks it up after "COPY . ." below, which
# defeats the point of restoring against project files alone.
COPY ["src/FantasyCritic.ClientAppVue2/FantasyCritic.ClientAppVue2.esproj", "src/FantasyCritic.ClientAppVue2/"]
COPY ["src/FantasyCritic.ClientAppVue2/package.json", "src/FantasyCritic.ClientAppVue2/package-lock.json", "src/FantasyCritic.ClientAppVue2/"]
# NSwag is a local tool, so its manifest has to be restored too.
COPY [".config/dotnet-tools.json", ".config/"]
RUN dotnet tool restore
RUN dotnet restore "src/FantasyCritic.Web/FantasyCritic.Web.csproj"

# Node modules get their own layer, keyed on the lock file, so a C# change does not reinstall
# them. The .esproj's EnsureNodeModules target then finds node_modules present and no-ops.
WORKDIR /src/src/FantasyCritic.ClientAppVue2
RUN npm ci
WORKDIR /src

COPY . .

# ------------------------------------------------------------------------------------------
# Build, generate the API clients, then publish.
# ------------------------------------------------------------------------------------------
# The order is not negotiable. nswag.json runs with noBuild, so it reads the already-compiled
# Web assembly to produce the OpenAPI document -- hence build first. And the generated
# TypeScript client is gitignored and imported by the Vue app, so vite cannot run until NSwag
# has written it -- hence publish last. Building the image without the middle step produces a
# broken bundle, or appears to work only because a developer's generated file was sitting in
# the build context.
RUN dotnet build "src/FantasyCritic.Web/FantasyCritic.Web.csproj" -c $BUILD_CONFIGURATION --no-restore

ENV NSWAG_CONFIGURATION=$BUILD_CONFIGURATION
RUN bash scripts/regenerate-api-client.sh

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FantasyCritic.Web/FantasyCritic.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Serilog writes rolling files here (LoggingPaths). Created and owned up front so the sinks
# work whether or not the host bind-mounts over it; a bind-mounted host directory must carry
# the same ownership, which is in the Phase 3 setup runbook.
USER root
RUN mkdir -p /var/log/fantasy-critic/web && chown -R $APP_UID /var/log/fantasy-critic
USER $APP_UID

# No connection string default. This image previously baked the development one, which meant a
# production misconfiguration would quietly connect somewhere wrong instead of failing.
# Compose supplies ConnectionStrings__DefaultConnection, or the app reads it from the
# environment's AWS Secrets Manager blob.
ENTRYPOINT ["dotnet", "FantasyCritic.Web.dll"]
