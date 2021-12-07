CREATE TABLE `tbl_settings_tradingsystem` (
    `TradingSystem` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`TradingSystem`) USING BTREE
)
ENGINE=InnoDB
ROW_FORMAT=DYNAMIC
;

INSERT INTO `tbl_settings_tradingsystem` (`TradingSystem`) VALUES ('NoTrades');
INSERT INTO `tbl_settings_tradingsystem` (`TradingSystem`) VALUES ('Standard');

ALTER TABLE `tbl_league_year`
    ADD COLUMN `TradingSystem` VARCHAR(50) NOT NULL DEFAULT 'NoTrades' AFTER `PlayStatus`,
    ADD CONSTRAINT `FK_tbl_league_year_tbl_settings_tradingsystem` 
        FOREIGN KEY (`TradingSystem`) REFERENCES `tbl_settings_tradingsystem` (`TradingSystem`) 
            ON UPDATE NO ACTION ON DELETE NO ACTION;

ALTER TABLE `tbl_league_year`
    CHANGE COLUMN `TradingSystem` `TradingSystem` VARCHAR(50) NOT NULL AFTER `PlayStatus`;
