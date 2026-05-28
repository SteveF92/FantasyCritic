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

-- Dumping structure for procedure fantasycritic.sp_getleaguesforuser
DROP PROCEDURE IF EXISTS `sp_getleaguesforuser`;
DELIMITER //
CREATE PROCEDURE `sp_getleaguesforuser`(
  IN `P_UserID` CHAR(36)
)
BEGIN 
  -- Main query with added fields UserIsInLeague and UserIsFollowingLeague
  -- TODO(Phase2-MultiDraft): Any implicit use of DraftNumber = 1 needs to be updated to something more robust once multi-draft is implemented.
  SELECT vw_league.*,
         tbl_league_hasuser.Archived,
         tbl_user.DisplayName AS ManagerDisplayName,
         tbl_user.EmailAddress AS ManagerEmailAddress,
         CASE
             WHEN most_recent_ly.OneShotMode = 1 THEN 1
             ELSE 0
         END AS MostRecentYearOneShot,
         CASE
             WHEN tbl_league_hasuser.UserID IS NOT NULL THEN 1
             ELSE 0
         END AS UserIsInLeague,
         CASE
             WHEN tbl_user_followingleague.UserID IS NOT NULL THEN 1
             ELSE 0
         END AS UserIsFollowingLeague,
         CASE
             WHEN EXISTS
                    (SELECT 1
                     FROM tbl_league_activeplayer ap
                     JOIN tbl_league_year ly ON ap.LeagueID = ly.LeagueID
                     WHERE ap.UserID = P_UserID
                       AND ap.LeagueID = vw_league.LeagueID
                       AND ap.Year =
                         (SELECT MAX(ly2.Year)
                          FROM tbl_league_year ly2
                          WHERE ly2.LeagueID = vw_league.LeagueID)) THEN 1
             ELSE 0
         END AS UserIsActiveInMostRecentYearForLeague,
         CASE
             WHEN EXISTS
                    (SELECT 1
                     FROM tbl_league_year ly
                     JOIN tbl_meta_supportedyear sy ON ly.Year = sy.Year
                     WHERE ly.LeagueID = vw_league.LeagueID
                       AND sy.OpenForPlay = 1) THEN 1
             ELSE 0
         END AS LeagueIsActiveInActiveYear,
         rg.GroupID AS LeagueRoyaleGroupID
  FROM vw_league
  LEFT JOIN tbl_league_hasuser ON vw_league.LeagueID = tbl_league_hasuser.LeagueID
  AND tbl_league_hasuser.UserID = P_UserID
  LEFT JOIN tbl_user_followingleague ON vw_league.LeagueID = tbl_user_followingleague.LeagueID
  AND tbl_user_followingleague.UserID = P_UserID
  LEFT JOIN
    (SELECT tbl_league_year.LeagueID,
            CASE
                WHEN tbl_league_year.EnableBids = 0
                     AND tbl_league_year.StandardGames = ld.GamesToDraft
                     AND tbl_league_year.CounterPicks = ld.CounterPicksToDraft
                     AND tbl_league_year.UnrestrictedReleaseStatusDroppableGames = 0
                     AND tbl_league_year.WillNotReleaseDroppableGames = 0
                     AND tbl_league_year.WillReleaseDroppableGames = 0
                     AND tbl_league_year.GrantSuperDrops = 0
                     AND tbl_league_year.TradingSystem = 'NoTrades' THEN 1
                ELSE 0
            END AS OneShotMode,
            ROW_NUMBER() OVER (PARTITION BY tbl_league_year.LeagueID
                               ORDER BY tbl_league_year.Year DESC) AS rn
     FROM tbl_league_year
     LEFT JOIN tbl_league_draft ld ON ld.LeagueID = tbl_league_year.LeagueID AND ld.Year = tbl_league_year.Year AND ld.DraftNumber = 1) AS most_recent_ly ON vw_league.LeagueID = most_recent_ly.LeagueID
  AND most_recent_ly.rn = 1
  LEFT JOIN tbl_royale_group rg ON rg.LeagueID = vw_league.LeagueID
  JOIN tbl_user ON tbl_user.UserID = vw_league.LeagueManager
  WHERE (tbl_league_hasuser.UserID IS NOT NULL
         OR tbl_user_followingleague.UserID IS NOT NULL)
    AND vw_league.IsDeleted = 0;
  
  -- Second result set
    SELECT tbl_league_year.LeagueID,
         tbl_league_year.Year,
         tbl_meta_supportedyear.Finished AS 'SupportedYearIsFinished',
         ld.PlayStatus,
         tbl_league_year.LeagueYearName
  FROM tbl_league_year
  JOIN tbl_league_hasuser ON tbl_league_year.LeagueID = tbl_league_hasuser.LeagueID
  JOIN tbl_meta_supportedyear ON tbl_league_year.Year = tbl_meta_supportedyear.`Year`
  JOIN tbl_league_draft ld ON ld.LeagueID = tbl_league_year.LeagueID AND ld.Year = tbl_league_year.Year AND ld.DraftNumber = 1
  WHERE tbl_league_hasuser.UserID = P_UserID
  UNION
  SELECT tbl_league_year.LeagueID,
         tbl_league_year.Year,
         tbl_meta_supportedyear.Finished AS 'SupportedYearIsFinished',
         ld.PlayStatus,
         tbl_league_year.LeagueYearName
  FROM tbl_league_year
  JOIN tbl_user_followingleague ON tbl_league_year.LeagueID = tbl_user_followingleague.LeagueID
  JOIN tbl_meta_supportedyear ON tbl_league_year.Year = tbl_meta_supportedyear.`Year`
  JOIN tbl_league_draft ld ON ld.LeagueID = tbl_league_year.LeagueID AND ld.Year = tbl_league_year.Year AND ld.DraftNumber = 1
  WHERE tbl_user_followingleague.UserID = P_UserID;
END//
DELIMITER ;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
