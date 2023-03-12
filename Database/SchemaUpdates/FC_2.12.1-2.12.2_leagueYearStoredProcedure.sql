CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getleagueyear`(
    IN `P_LeagueID` CHAR(36),
    IN `P_Year` SMALLINT
)
LANGUAGE SQL
NOT DETERMINISTIC
CONTAINS SQL
SQL SECURITY DEFINER
COMMENT ''
BEGIN
    -- League
    select * from vw_league where LeagueID = P_LeagueID and IsDeleted = 0;
    
    select Year from tbl_league_year where LeagueID = P_LeagueID;
    
    -- League Year
    select * from tbl_league_year where LeagueID = P_LeagueID and Year = P_Year;
    
    select * from tbl_league_yearusestag where LeagueID = P_LeagueID AND YEAR = P_Year;
    
    select * from tbl_league_specialgameslot where LeagueID = P_LeagueID AND Year = P_Year;
    
    select * from tbl_league_eligibilityoverride where LeagueID = P_LeagueID and Year = P_Year;
    
    select tbl_league_tagoverride.* from tbl_league_tagoverride
    JOIN tbl_mastergame_tag ON tbl_league_tagoverride.TagName = tbl_mastergame_tag.Name
    WHERE LeagueID = P_LeagueID AND Year = P_Year;
    
    -- Publishers
    select tbl_user.* from tbl_user join tbl_league_hasuser on (tbl_user.UserID = tbl_league_hasuser.UserID) where tbl_league_hasuser.LeagueID = P_LeagueID;
    
    select * from tbl_league_publisher where tbl_league_publisher.LeagueID = P_LeagueID and tbl_league_publisher.Year = P_Year;
    
    select tbl_league_publishergame.* from tbl_league_publishergame
    join tbl_league_publisher on (tbl_league_publishergame.PublisherID = tbl_league_publisher.PublisherID)
    join tbl_league_year on (tbl_league_year.LeagueID = tbl_league_publisher.LeagueID AND tbl_league_year.Year = tbl_league_publisher.Year)
    where tbl_league_year.LeagueID = P_LeagueID AND tbl_league_year.Year = P_Year;
    
   select tbl_league_formerpublishergame.* from tbl_league_formerpublishergame
    join tbl_league_publisher on (tbl_league_formerpublishergame.PublisherID = tbl_league_publisher.PublisherID)
    join tbl_league_year on (tbl_league_year.LeagueID = tbl_league_publisher.LeagueID AND tbl_league_year.Year = tbl_league_publisher.Year)
    where tbl_league_year.LeagueID = P_LeagueID AND tbl_league_year.Year = P_Year;
END