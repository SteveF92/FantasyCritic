-- --------------------------------------------------------
-- Host:                         fantasy-critic-rds.cldutembgs4w.us-east-1.rds.amazonaws.com
-- Server version:               8.4.7 - Source distribution
-- Server OS:                    Linux
-- HeidiSQL Version:             12.3.0.6589
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping structure for procedure fantasycritic.sp_getroyalegroupmemberdisplayrows
DROP PROCEDURE IF EXISTS `sp_getroyalegroupmemberdisplayrows`;
DELIMITER //
CREATE PROCEDURE `sp_getroyalegroupmemberdisplayrows`(
  IN `P_GroupID` CHAR(36),
  IN `P_Year` INT,
  IN `P_Quarter` INT
)
BEGIN
  DECLARE v_prev_year INT;
  DECLARE v_prev_quarter INT;

  IF P_Quarter = 1 THEN
    SET v_prev_year = P_Year - 1;
    SET v_prev_quarter = 4;
  ELSE
    SET v_prev_year = P_Year;
    SET v_prev_quarter = P_Quarter - 1;
  END IF;

  -- 1. Group (0 or 1 row)
  SELECT g.*, u.DisplayName AS ManagerDisplayName
  FROM tbl_royale_group g
  LEFT JOIN tbl_user u ON g.ManagerUserID = u.UserID
  WHERE g.GroupID = P_GroupID;

  -- 2. Members
  SELECT u.UserID, u.DisplayName
  FROM tbl_royale_group_member gm
  JOIN tbl_user u ON gm.UserID = u.UserID
  WHERE gm.GroupID = P_GroupID
  ORDER BY u.DisplayName;

  -- 3. Requested supported quarter (0 or 1 row)
  SELECT tbl_royale_supportedquarter.*,
         tbl_user.DisplayName AS WinningUserDisplayName
  FROM tbl_royale_supportedquarter
  LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
  WHERE tbl_royale_supportedquarter.Year = P_Year
    AND tbl_royale_supportedquarter.Quarter = P_Quarter;

  -- 4. Previous calendar quarter as a supported quarter (0 or 1 row)
  SELECT tbl_royale_supportedquarter.*,
         tbl_user.DisplayName AS WinningUserDisplayName
  FROM tbl_royale_supportedquarter
  LEFT JOIN tbl_user ON tbl_royale_supportedquarter.WinningUser = tbl_user.UserID
  WHERE tbl_royale_supportedquarter.Year = v_prev_year
    AND tbl_royale_supportedquarter.Quarter = v_prev_quarter;

  -- 5. Current-quarter publishers for group members
  SELECT p.*, u.DisplayName AS PublisherDisplayName
  FROM tbl_royale_publisher p
  JOIN tbl_user u ON u.UserID = p.UserID
  WHERE p.Year = P_Year
    AND p.Quarter = P_Quarter
    AND p.UserID IN (
      SELECT UserID FROM tbl_royale_group_member WHERE GroupID = P_GroupID
    );

  -- 6. Group members who have a publisher in the previous quarter (for visibility when current is null)
  SELECT DISTINCT p.UserID
  FROM tbl_royale_publisher p
  WHERE p.Year = v_prev_year
    AND p.Quarter = v_prev_quarter
    AND p.UserID IN (
      SELECT UserID FROM tbl_royale_group_member WHERE GroupID = P_GroupID
    );

  -- 7. Publisher games for current-quarter group publishers
  SELECT pg.*
  FROM tbl_royale_publishergame pg
  JOIN tbl_royale_publisher p ON p.PublisherID = pg.PublisherID
  WHERE p.Year = P_Year
    AND p.Quarter = P_Quarter
    AND p.UserID IN (
      SELECT UserID FROM tbl_royale_group_member WHERE GroupID = P_GroupID
    );

  -- Master game data (same pattern as sp_getroyaleyearquarterdata)

  SELECT *
  FROM tbl_mastergame_tag;


  SELECT *
  FROM tbl_mastergame_subgame;


  SELECT *
  FROM tbl_mastergame_hastag;


  SELECT tbl_caching_mastergameyear.*,
         tbl_user.DisplayName AS AddedByUserDisplayName
  FROM tbl_caching_mastergameyear
  JOIN tbl_user ON tbl_user.UserID = tbl_caching_mastergameyear.AddedByUserID
  WHERE tbl_caching_mastergameyear.`Year` = P_Year;

  -- Statistics for current-quarter group publishers
  SELECT ps.*
  FROM tbl_royale_publisherstatistics ps
  WHERE ps.PublisherID IN (
    SELECT p.PublisherID
    FROM tbl_royale_publisher p
    WHERE p.Year = P_Year
      AND p.Quarter = P_Quarter
      AND p.UserID IN (
        SELECT UserID FROM tbl_royale_group_member WHERE GroupID = P_GroupID
      )
  )
  ORDER BY ps.PublisherID, ps.Date ASC;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
