echo Stopping service
sudo systemctl stop fantasy-critic.service

# Read the environment from the service unit so this script works on both beta and prod.
ASPNETCORE_ENVIRONMENT=$(systemctl show fantasy-critic.service -p Environment --value | tr ' ' '\n' | grep '^ASPNETCORE_ENVIRONMENT=' | cut -d= -f2)
ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
echo "Using ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"

echo Building and running database updater
DBUP_AREA=../DbUpArea
rm -rf folderName "$DBUP_AREA"
dotnet publish src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj -c Release -o "$DBUP_AREA"
DBUP_PUBLISH_EXIT_CODE=$?
if [ $DBUP_PUBLISH_EXIT_CODE -ne 0 ]; then
  echo "Database updater publish failed (exit code: $DBUP_PUBLISH_EXIT_CODE). Not restarting site."
  exit $DBUP_PUBLISH_EXIT_CODE
fi
(cd "$DBUP_AREA" && ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENVIRONMENT" dotnet FantasyCritic.DatabaseUpdater.dll)
DBUP_EXIT_CODE=$?
if [ $DBUP_EXIT_CODE -ne 0 ]; then
  echo "Database migration failed (exit code: $DBUP_EXIT_CODE). Not restarting site."
  exit $DBUP_EXIT_CODE
fi

echo Restoring dotnet tools and regenerating API client
dotnet tool restore
dotnet build src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release
BUILD_EXIT_CODE=$?
if [ $BUILD_EXIT_CODE -ne 0 ]; then
  echo "Web project build failed (exit code: $BUILD_EXIT_CODE). Not restarting site."
  exit $BUILD_EXIT_CODE
fi
NSWAG_CONFIGURATION=Release bash scripts/regenerate-api-client.sh
NSWAG_EXIT_CODE=$?
if [ $NSWAG_EXIT_CODE -ne 0 ]; then
  echo "API client generation failed (exit code: $NSWAG_EXIT_CODE). Not restarting site."
  exit $NSWAG_EXIT_CODE
fi

echo Building
rm -rf folderName ../BuildArea
dotnet publish src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release -o ../BuildArea
PUBLISH_EXIT_CODE=$?
if [ $PUBLISH_EXIT_CODE -ne 0 ]; then
  echo "Web publish failed (exit code: $PUBLISH_EXIT_CODE). Not restarting site."
  exit $PUBLISH_EXIT_CODE
fi

echo Deleting and moving files
rm -rf /var/www/fantasy-critic/*
cp -a ../BuildArea/. /var/www/fantasy-critic/
rm -rf folderName ../BuildArea

echo Starting site
sudo systemctl start fantasy-critic.service
echo Site started