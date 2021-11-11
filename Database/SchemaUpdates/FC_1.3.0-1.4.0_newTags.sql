INSERT INTO `tbl_mastergame_tag` (`Name`, `ReadableName`, `ShortName`, `TagType`, `HasCustomCode`, `Description`, `Examples`, `BadgeColor`) VALUES ('VirtualReality', 'Virtual Reality', 'VR', 'Other', b'0', 'A game designed for a VR platform.', '["Superhot VR", "Lone Echo 2", "Resident Evil 4 VR"]', '1C1E20');

INSERT INTO tbl_mastergame_hastag(MasterGameID, TagName)
SELECT tbl_mastergame.MasterGameID, "VirtualReality" AS TagName FROM tbl_mastergame 
JOIN tbl_mastergame_hastag ON tbl_mastergame.MasterGameID = tbl_mastergame_hastag.MasterGameID
WHERE TagName = "VRPort";

INSERT INTO tbl_mastergame_hastag(MasterGameID, TagName)
SELECT tbl_mastergame.MasterGameID, "Remake" AS TagName FROM tbl_mastergame 
JOIN tbl_mastergame_hastag ON tbl_mastergame.MasterGameID = tbl_mastergame_hastag.MasterGameID
WHERE TagName = "VRPort";

DELETE FROM tbl_mastergame_hastag WHERE TagName = "VRPort";
DELETE FROM tbl_league_yearusestag WHERE Tag = "VRPort";
DELETE FROM tbl_mastergame_tag WHERE Name = "VRPort";

INSERT INTO `tbl_mastergame_tag` (`Name`, `ReadableName`, `ShortName`, `TagType`, `HasCustomCode`, `Description`, `Examples`, `BadgeColor`) VALUES ('PartialRemake', 'Partial Remake', 'P-RMKE', 'RemakeLevel', b'0', 'This category is hard to define. There are a lot of games that are a little "too remade" to be a remaster but "too similiar" to be a remake. They live here.', '["Demon\'s Souls (2020)", "Shadow of the Colossus (2018)", "Diablo 2: Resurrected", "Grand Theft Auto: The Trilogy", "The Legend of Zelda: Skyward Sword HD"]', 'FD9C11');
INSERT INTO `tbl_mastergame_tag` (`Name`, `ReadableName`, `ShortName`, `TagType`, `HasCustomCode`, `Description`, `Examples`, `BadgeColor`) VALUES ('NewGamingFranchise', 'New Gaming Franchise', 'NGF', 'Other', b'0', 'A game that begins or new series, or a standalone game. Not a sequel. Can be from an existing IP if the IP has never been represented in video games before.', '["Celeste", "Death Loop", "Elden Ring", "Back 4 Blood", "Metro 2033"]', '036BFC');

UPDATE `fantasycritic`.`tbl_mastergame_tag` SET `Examples`='["The Legend of Zelda: Link\'s Awakening (Switch)","Crash N-Sane Trilogy","Pokemon Brilliant Diamond and Shining Pearl"]' WHERE  `Name`='Remake';