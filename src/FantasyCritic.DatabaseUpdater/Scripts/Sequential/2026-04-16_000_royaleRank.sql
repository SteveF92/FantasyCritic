ALTER TABLE `tbl_royale_publisher`
	ADD COLUMN `Ranking` INT NULL AFTER `Budget`;

UPDATE tbl_royale_publisher rp
LEFT JOIN (
    SELECT
        ranked.`Year`,
        ranked.`Quarter`,
        ranked.PublisherID,
        ranked.CalculatedRank
    FROM (
        SELECT
            rp2.`Year`,
            rp2.`Quarter`,
            rp2.PublisherID,
            RANK() OVER (
                PARTITION BY rp2.`Year`, rp2.`Quarter`
                ORDER BY SUM(rpg.FantasyPoints) DESC
            ) AS CalculatedRank
        FROM tbl_royale_publisher rp2
        JOIN tbl_royale_publishergame rpg
            ON rp2.PublisherID = rpg.PublisherID
           AND rpg.FantasyPoints IS NOT NULL
        JOIN tbl_royale_supportedquarter rsq
            ON rp2.`Year` = rsq.`Year`
           AND rp2.`Quarter` = rsq.`Quarter`
        WHERE rsq.Finished = 1
        GROUP BY
            rp2.`Year`,
            rp2.`Quarter`,
            rp2.PublisherID
    ) ranked
) r
    ON rp.`Year` = r.`Year`
   AND rp.`Quarter` = r.`Quarter`
   AND rp.PublisherID = r.PublisherID
JOIN tbl_royale_supportedquarter rsq
    ON rp.`Year` = rsq.`Year`
   AND rp.`Quarter` = rsq.`Quarter`
SET rp.Ranking = r.CalculatedRank
WHERE rsq.Finished = 1;
