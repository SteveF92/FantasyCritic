-- Dumping structure for procedure fantasycritic.sp_getbasicdata
DROP PROCEDURE IF EXISTS `sp_getbasicdata`;
DELIMITER //
CREATE PROCEDURE `sp_getbasicdata`()
BEGIN
	select * from tbl_meta_systemwidesettings;
	select * from tbl_mastergame_tag;
	select * from tbl_meta_supportedyear;
END//
DELIMITER ;