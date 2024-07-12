-- You must first fix the Duplicate royale publishers. This select will help:
SELECT tbl_royale_publisher.* FROM
(SELECT  `UserID`, `Year`, `Quarter`, COUNT(*) AS Num FROM tbl_royale_publisher GROUP BY `UserID`, `Year`, `Quarter`) sub
JOIN tbl_royale_publisher ON tbl_royale_publisher.UserID = sub.UserID AND tbl_royale_publisher.Year = sub.Year AND tbl_royale_publisher.Quarter = sub.Quarter
WHERE sub.Num > 1 -- AND tbl_royale_publisher.Budget = 100

-- Once this Index can be created, we're done
ALTER TABLE `tbl_royale_publisher`
	ADD UNIQUE INDEX `UNQ_Publisher` (`UserID`, `Year`, `Quarter`);
