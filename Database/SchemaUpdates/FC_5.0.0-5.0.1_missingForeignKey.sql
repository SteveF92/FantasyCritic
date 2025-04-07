ALTER TABLE `tbl_royale_supportedquarter`
	ADD CONSTRAINT `FK_tbl_royale_supportedquarter_tbl_user` FOREIGN KEY (`WinningUser`) REFERENCES `tbl_user` (`UserID`) ON UPDATE NO ACTION ON DELETE NO ACTION;
