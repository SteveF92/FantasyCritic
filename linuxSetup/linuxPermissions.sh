mkdir -p /var/log/fantasy-critic
mkdir -p /var/lib/fantasy-critic
mkdir -p /var/www/fantasy-critic

chown -R ubuntu /var/log/fantasy-critic/
chown -R ubuntu /var/lib/fantasy-critic/
chown -R ubuntu /var/www/fantasy-critic/

chgrp -R www-data /var/log/fantasy-critic/
chgrp -R www-data /var/lib/fantasy-critic/
chgrp -R www-data /var/www/fantasy-critic/

chmod -R 770 /var/log/fantasy-critic/
chmod -R 770 /var/lib/fantasy-critic/
chmod -R 770 /var/www/fantasy-critic/

echo Permissions Set!
