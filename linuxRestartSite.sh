echo Stopping service
sudo systemctl stop fantasy-critic.service
echo Restarting...
sudo systemctl start fantasy-critic.service
echo Site started