echo Building
dotnet publish src/FantasyCritic.Web/FantasyCritic.Web.csproj -c Release -o BuildArea

echo Build done, stopping service
sudo systemctl stop fantasy-critic.service

echo Deleting and moving files
rm -r /var/www/fantasy-critic/*
mv  -v BuildArea/* /var/www/fantasy-critic/
rm -rf folderName BuildArea

echo Starting site
sudo systemctl start fantasy-critic.service
echo Site started