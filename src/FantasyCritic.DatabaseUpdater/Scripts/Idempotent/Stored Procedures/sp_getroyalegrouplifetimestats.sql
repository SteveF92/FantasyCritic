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

-- Dumping structure for procedure fantasycritic.sp_getroyalegrouplifetimestats
DROP PROCEDURE IF EXISTS `sp_getroyalegrouplifetimestats`;
DELIMITER //
CREATE PROCEDURE `sp_getroyalegrouplifetimestats`(
  IN `P_GroupID` CHAR(36)
)
BEGIN
  -- 1. Group (0 or 1 row)
  SELECT g.*, u.DisplayName AS ManagerDisplayName
  FROM tbl_royale_group g
  LEFT JOIN tbl_user u ON g.ManagerUserID = u.UserID
  WHERE g.GroupID = P_GroupID;

  -- 2. Members (same order as GetRoyaleGroupMembers)
  SELECT u.UserID, u.DisplayName
  FROM tbl_royale_group_member gm
  JOIN tbl_user u ON gm.UserID = u.UserID
  WHERE gm.GroupID = P_GroupID
  ORDER BY u.DisplayName;

  -- 3. Per-member lifetime aggregates (visibility, points, and rank mirror GetRoyaleGroupMemberDisplayRows + loop)
  WITH gm AS (
    SELECT u.UserID, u.DisplayName
    FROM tbl_royale_group_member g
    JOIN tbl_user u ON u.UserID = g.UserID
    WHERE g.GroupID = P_GroupID
  ),
  base AS (
    SELECT
      sq.Year,
      sq.Quarter,
      (CAST(sq.Finished AS UNSIGNED) = 1) AS is_finished,
      m.UserID,
      EXISTS (
        SELECT 1
        FROM tbl_royale_publisher p
        WHERE p.UserID = m.UserID
          AND p.Year = sq.Year
          AND p.Quarter = sq.Quarter
      ) AS has_curr,
      EXISTS (
        SELECT 1
        FROM tbl_royale_supportedquarter ps
        WHERE ps.Year = IF(sq.Quarter = 1, sq.Year - 1, sq.Year)
          AND ps.Quarter = IF(sq.Quarter = 1, 4, sq.Quarter - 1)
      ) AS prev_supported,
      EXISTS (
        SELECT 1
        FROM tbl_royale_publisher p
        WHERE p.UserID = m.UserID
          AND p.Year = IF(sq.Quarter = 1, sq.Year - 1, sq.Year)
          AND p.Quarter = IF(sq.Quarter = 1, 4, sq.Quarter - 1)
      ) AS has_prev
    FROM tbl_royale_supportedquarter sq
    CROSS JOIN gm m
  ),
  visible AS (
    SELECT *,
      (is_finished AND has_curr)
      OR ((NOT is_finished) AND (NOT prev_supported))
      OR ((NOT is_finished) AND prev_supported AND (has_curr OR has_prev)) AS in_display
    FROM base
  ),
  filtered AS (
    SELECT Year, Quarter, UserID, has_curr, has_prev, is_finished
    FROM visible
    WHERE in_display
  ),
  pub_pts AS (
    SELECT
      f.Year,
      f.Quarter,
      f.UserID,
      p.PublisherID,
      COALESCE(gp.game_pts, 0) AS total_pts
    FROM filtered f
    LEFT JOIN tbl_royale_publisher p
      ON p.UserID = f.UserID
     AND p.Year = f.Year
     AND p.Quarter = f.Quarter
    LEFT JOIN (
      SELECT PublisherID, COALESCE(SUM(FantasyPoints), 0) AS game_pts
      FROM tbl_royale_publishergame
      GROUP BY PublisherID
    ) gp ON gp.PublisherID = p.PublisherID
  ),
  positive_ranks AS (
    SELECT
      Year,
      Quarter,
      UserID,
      total_pts,
      ROW_NUMBER() OVER (
        PARTITION BY Year, Quarter
        ORDER BY total_pts DESC, UserID
      ) AS quarter_rank
    FROM pub_pts
    WHERE PublisherID IS NOT NULL
      AND total_pts > 0
  ),
  global_pub_pts AS (
    SELECT
      p.Year,
      p.Quarter,
      p.UserID,
      p.PublisherID,
      COALESCE(gp.game_pts, 0) AS total_pts
    FROM tbl_royale_publisher p
    LEFT JOIN (
      SELECT PublisherID, COALESCE(SUM(FantasyPoints), 0) AS game_pts
      FROM tbl_royale_publishergame
      GROUP BY PublisherID
    ) gp ON gp.PublisherID = p.PublisherID
  ),
  global_positive_ranks AS (
    SELECT
      Year,
      Quarter,
      UserID,
      total_pts,
      ROW_NUMBER() OVER (
        PARTITION BY Year, Quarter
        ORDER BY total_pts DESC, UserID
      ) AS global_quarter_rank
    FROM global_pub_pts
    WHERE PublisherID IS NOT NULL
      AND total_pts > 0
  )
  SELECT
    m.UserID,
    COALESCE(SUM(CASE WHEN pp.PublisherID IS NOT NULL THEN 1 ELSE 0 END), 0) AS QuartersParticipated,
    COALESCE(SUM(CASE WHEN pp.PublisherID IS NOT NULL THEN pp.total_pts ELSE 0 END), 0) AS TotalPoints,
    AVG(pr.quarter_rank) AS AverageRankWithinGroup,
    AVG(gpr.global_quarter_rank) AS AverageRankOverall
  FROM gm m
  LEFT JOIN pub_pts pp ON pp.UserID = m.UserID
  LEFT JOIN positive_ranks pr
    ON pr.Year = pp.Year
   AND pr.Quarter = pp.Quarter
   AND pr.UserID = pp.UserID
  LEFT JOIN global_positive_ranks gpr
    ON gpr.Year = pp.Year
   AND gpr.Quarter = pp.Quarter
   AND gpr.UserID = pp.UserID
  GROUP BY m.UserID, m.DisplayName
  ORDER BY m.DisplayName;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
