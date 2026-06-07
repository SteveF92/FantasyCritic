-- Seed lookup values for the AutoDraftMode enum.
-- These rows are required by the FK on tbl_league_publisher.AutoDraftMode.
INSERT IGNORE INTO `tbl_meta_autodraftmode` (`Mode`) VALUES
  ('Off'),
  ('StandardGamesOnly'),
  ('All');
