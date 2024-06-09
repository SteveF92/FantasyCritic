-- Dumping structure for procedure fantasycritic.sp_getbasicdata
DROP PROCEDURE IF EXISTS `sp_getbasicdata`;
DELIMITER //
CREATE PROCEDURE `sp_getbasicdata`(
	IN `P_UserID` CHAR(36)
)
BEGIN
	select * from tbl_user WHERE UserID = P_UserID;
	select * from tbl_meta_systemwidesettings;
	select * from tbl_mastergame_tag;
	select * from tbl_meta_supportedyear;
END//
DELIMITER ;