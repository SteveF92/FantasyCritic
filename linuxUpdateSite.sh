echo Stopping service
sudo systemctl stop fantasy-critic.service

echo Building and running database updater
DBUP_AREA=../DbUpArea
rm -rf folderName "$DBUP_AREA"
dotnet publish src/FantasyCritic.DatabaseUpdater/FantasyCritic.DatabaseUpdater.csproj -c Release -o "$DBUP_AREA"
DBUP_PUBLISH_EXIT_CODE=$?
if [ $DBUP_PUBLISH_EXIT_CODE -ne 0 ]; then
  echo "Database updater publish failed (exit code: $DBUP_PUBLISH_EXIT_CODE). Not restarting site."
  exit $DBUP_PUBLISH_EXIT_CODE
fi
(cd "$DBUP_AREA" && dotnet FantasyCritic.DatabaseUpdater.dll)
DBUP_EXIT_CODE=$?
if [ $DBUP_EXIT_CODE -ne 0 ]; then
  echo "Database migration failed (exit code: $DBUP_EXIT_CODE). Not restarting site."
  exit $DBUP_EXIT_CODE
fi

echo Building
rm -rf folderName ../BuildArea
dotnet publish src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release -o ../BuildArea

echo Deleting and moving files
rm -rf /var/www/fantasy-critic/*
cp -a ../BuildArea/. /var/www/fantasy-critic/
rm -rf folderName ../BuildArea

echo Starting site
sudo systemctl start fantasy-critic.service
echo Site started