mkdir -p /var/log/fantasy-critic
mkdir -p /var/lib/fantasy-critic
mkdir -p /var/www/fantasy-critic

chown -R ec2-user /var/log/fantasy-critic/
chown -R ec2-user /var/lib/fantasy-critic/
chown -R ec2-user /var/www/fantasy-critic/

chgrp -R nginx /var/log/fantasy-critic/
chgrp -R nginx /var/lib/fantasy-critic/
chgrp -R nginx /var/www/fantasy-critic/

chmod -R 770 /var/log/fantasy-critic/
chmod -R 770 /var/lib/fantasy-critic/
chmod -R 770 /var/www/fantasy-critic/

echo Permissions Set!
