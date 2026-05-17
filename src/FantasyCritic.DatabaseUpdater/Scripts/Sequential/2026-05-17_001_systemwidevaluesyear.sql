CREATE TABLE IF NOT EXISTS `tbl_caching_systemwidevaluesyear` (
  `Year` year NOT NULL,
  `AverageStandardGamePoints` decimal(12,9) NOT NULL,
  `StandardGameDataPoints` int unsigned NOT NULL,
  `AveragePickupOnlyStandardGamePoints` decimal(12,9) NOT NULL,
  `PickupOnlyDataPoints` int unsigned NOT NULL,
  `AverageCounterPickPoints` decimal(12,9) NOT NULL,
  `CounterPickDataPoints` int unsigned NOT NULL,
  PRIMARY KEY (`Year`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tbl_caching_averagepositionpointsyear` (
  `Year` year NOT NULL,
  `PickPosition` int unsigned NOT NULL,
  `DataPoints` int unsigned NOT NULL,
  `AveragePoints` decimal(12,9) NOT NULL,
  PRIMARY KEY (`Year`, `PickPosition`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `tbl_caching_averagebidamountpointsyear` (
  `Year` year NOT NULL,
  `BidAmount` int unsigned NOT NULL,
  `DataPoints` int unsigned NOT NULL,
  `AveragePoints` decimal(12,9) NOT NULL,
  PRIMARY KEY (`Year`, `BidAmount`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
