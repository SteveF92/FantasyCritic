DELETE tbl_royale_publisherstatistics
FROM tbl_royale_publisherstatistics
JOIN tbl_royale_publisher 
  ON tbl_royale_publisher.PublisherID = tbl_royale_publisherstatistics.PublisherID
WHERE tbl_royale_publisher.`Year` = 2026 
  AND tbl_royale_publisher.`Quarter` = 2 
  AND tbl_royale_publisherstatistics.Date < '2026-04-01';
