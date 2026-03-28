# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Node.js + npm for ClientApp publish (see PublishRunWebpack in FantasyCritic.Web.csproj)
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FantasyCritic.Web/FantasyCritic.Web.csproj", "FantasyCritic.Web/"]
COPY ["FantasyCritic.AWS/FantasyCritic.AWS.csproj", "FantasyCritic.AWS/"]
COPY ["FantasyCritic.Lib/FantasyCritic.Lib.csproj", "FantasyCritic.Lib/"]
COPY ["FantasyCritic.EmailTemplates/FantasyCritic.EmailTemplates.csproj", "FantasyCritic.EmailTemplates/"]
COPY ["FantasyCritic.FakeRepo/FantasyCritic.FakeRepo.csproj", "FantasyCritic.FakeRepo/"]
COPY ["FantasyCritic.MySQL/FantasyCritic.MySQL.csproj", "FantasyCritic.MySQL/"]
COPY ["FantasyCritic.Postmark/FantasyCritic.Postmark.csproj", "FantasyCritic.Postmark/"]
RUN dotnet restore "./FantasyCritic.Web/FantasyCritic.Web.csproj"
COPY . .
WORKDIR "/src/FantasyCritic.Web"
RUN dotnet build "./FantasyCritic.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FantasyCritic.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Published appsettings use localhost:3307 for the host machine; inside a container that is wrong.
# Compose (or `docker run -e`) should override this; this default matches the `mysql` service name on the Compose network.
ENV ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=fantasycritic;Uid=fantasycritic;Pwd=afantasticpassword;SslMode=required;charset=utf8;"
ENTRYPOINT ["dotnet", "FantasyCritic.Web.dll"]