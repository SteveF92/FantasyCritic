SELECT 
	DATE(CONVERT_TZ(tbl_meta_actionprocessingset.ProcessTime, 'UTC', 'America/New_York')) AS ProcessDate,
	tbl_mastergame.MasterGameID, tbl_mastergame.GameName, 
	COUNT(*) AS TotalBidCount,
	SUM(if(tbl_league_pickupbid.Successful = 1, 1, 0)) AS SuccessfulBids,
	SUM(if(tbl_league_pickupbid.Successful = 0, 1, 0)) AS FailedBids,
	COUNT(DISTINCT tbl_league_publisher.LeagueID) AS TotalLeagues,
	SUM(tbl_league_pickupbid.BidAmount) AS TotalAmount
FROM tbl_league_pickupbid
	JOIN tbl_league_publisher ON tbl_league_pickupbid.PublisherID = tbl_league_publisher.PublisherID
	JOIN tbl_mastergame ON tbl_league_pickupbid.MasterGameID = tbl_mastergame.MasterGameID
	JOIN tbl_meta_actionprocessingset ON tbl_league_pickupbid.ProcessSetID = tbl_meta_actionprocessingset.ProcessSetID
GROUP BY ProcessDate, tbl_mastergame.MasterGameID, tbl_mastergame.GameName
ORDER BY ProcessDate, TotalBidCount DESC;

SELECT 
	DATE(CONVERT_TZ(tbl_meta_actionprocessingset.ProcessTime, 'UTC', 'America/New_York')) AS ProcessDate,
	tbl_mastergame.MasterGameID, tbl_mastergame.GameName, 
	COUNT(*) AS TotalDropCount
FROM tbl_league_droprequest
	JOIN tbl_league_publisher ON tbl_league_droprequest.PublisherID = tbl_league_publisher.PublisherID
	JOIN tbl_mastergame ON tbl_league_droprequest.MasterGameID = tbl_mastergame.MasterGameID
	JOIN tbl_meta_actionprocessingset ON tbl_league_droprequest.ProcessSetID = tbl_meta_actionprocessingset.ProcessSetID
GROUP BY ProcessDate, tbl_mastergame.MasterGameID, tbl_mastergame.GameName
ORDER BY ProcessDate, TotalDropCount DESC;