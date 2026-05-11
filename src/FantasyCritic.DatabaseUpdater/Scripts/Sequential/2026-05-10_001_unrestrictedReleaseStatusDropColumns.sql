ALTER TABLE `tbl_league_year`
  RENAME COLUMN `FreeDroppableGames` TO `UnrestrictedReleaseStatusDroppableGames`;

ALTER TABLE `tbl_league_publisher`
  RENAME COLUMN `FreeGamesDropped` TO `UnrestrictedReleaseStatusGamesDropped`;
