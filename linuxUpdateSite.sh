echo Stopping service
sudo systemctl stop fantasy-critic.service

echo Building
dotnet publish src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release -o ../BuildArea

echo Deleting and moving files
rm -rf /var/www/fantasy-critic/*
cp -a ../BuildArea/. /var/www/fantasy-critic/
rm -rf folderName ../BuildArea

echo Starting site
sudo systemctl start fantasy-critic.service
echo Site started