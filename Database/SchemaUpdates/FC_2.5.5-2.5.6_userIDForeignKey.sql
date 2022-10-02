ALTER TABLE `tbl_mastergame`
	ADD CONSTRAINT `FK_tbl_mastergame_tbl_user` FOREIGN KEY (`AddedByUserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_caching_mastergameyear`
	ADD CONSTRAINT `FK_tbl_caching_mastergameyear_tbl_user` FOREIGN KEY (`AddedByUserID`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION;
