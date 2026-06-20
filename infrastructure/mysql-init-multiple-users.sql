-- Runs automatically on first container initialization.
-- Creates MySQL users required by the app and grants required privileges.

CREATE DATABASE IF NOT EXISTS fantasycritic;
CREATE DATABASE IF NOT EXISTS `fantasycritic-fromsnapshot`;

-- App user
CREATE USER IF NOT EXISTS 'fantasycritic'@'%' IDENTIFIED BY 'afantasticpassword';
ALTER USER 'fantasycritic'@'%' IDENTIFIED BY 'afantasticpassword';
-- Ensure privileges match the app's required capabilities (not "all").
REVOKE ALL PRIVILEGES, GRANT OPTION FROM 'fantasycritic'@'%';

-- App admin
CREATE USER IF NOT EXISTS 'fantasycritic-admin'@'%' IDENTIFIED BY 'anotherfantasticpassword';
ALTER USER 'fantasycritic-admin'@'%' IDENTIFIED BY 'anotherfantasticpassword';

-- fantasycritic: data access required by the app
GRANT EXECUTE, SELECT, SHOW VIEW, DELETE, INSERT, UPDATE
  ON fantasycritic.* TO 'fantasycritic'@'%';

GRANT EXECUTE, SELECT, SHOW VIEW, DELETE, INSERT, UPDATE
  ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic'@'%';

-- fantasycritic-admin: full access on both local databases + minimal global read for tooling
-- WITH GRANT OPTION lets fantasycritic-admin run GRANT statements (e.g. in DbUp migrations).
GRANT ALL PRIVILEGES ON fantasycritic.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;
GRANT ALL PRIVILEGES ON `fantasycritic-fromsnapshot`.* TO 'fantasycritic-admin'@'%' WITH GRANT OPTION;
GRANT SELECT, SHOW DATABASES, SHOW VIEW ON *.* TO 'fantasycritic-admin'@'%';

FLUSH PRIVILEGES;
