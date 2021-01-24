select * from GroupFixedCatValues;
select F.Name, G.Name, F.CountryName, F.Address, GF.* from GroupFriends GF
	join Friends F ON F.Id = GF.FriendId
	join Groups G ON G.Id = GF.GroupId
order by GF.Id;
select * from Groups;
select * from Friends;
select * from Friends where Info like 'CurrentVersion=1.0.10.10%';
select * from Friends where Location is null;
select * from GroupPredefinedCategory;
select * from CacheConfiguration;
select * from Notification;
select * from settings order by [order];
SELECT Id, FriendId, ModifiedDate, SharingInfo, location.STY as Lat, location.STX as Lon from FriendLocationsHistory order by Id desc

drop table GroupFixedCatValues;
drop table GroupFriends;
drop table Friends;
drop table GroupPredefinedCategory;
drop table Groups;
drop table CacheConfiguration;
drop table Notification;
drop table Settings;

--ALTER TABLE GroupFriends ADD [Id] [int] IDENTITY(1,1) NOT NULL
--UPDATE Groups SET Active = 0 WHERE Name = 'Amser9497';

select * from Friends where Info like '%1.2.5%' or Info like '%1.2.3%'

UPDATE CONFIGURATION SET Enabled = 0 WHERE NOT [KEY] LIKE 'CacheTimeout.%'
Insert Into Configuration([Key], [Value], Enabled, Comment) Values('MAPONLINE_ACTIVE_TIMEOUT', '2', 1, 'in minute')
Insert Into Configuration([Key], [Value], Enabled, Comment) Values('MAPONLINE_ONLINE_TIMEOUT', '4', 1, 'in minute')
Insert Into Configuration([Key], [Value], Enabled, Comment) Values('MAPONLINE_RADIUS_LIST', '0.2;0.5;1;10;50;0', 1, 'list in km')
Insert Into Configuration([Key], [Value], Enabled) Values('SuperUserIds', '', 1)
Insert Into Configuration([Key], [Value], Enabled, Comment) Values('LOCATIONSERVICE_UPDATE_INTERVAL', '15', 1, 'in second')
update Configuration set [Value] = '2' WHERE [Key] = 'MAPONLINE_ACTIVE_TIMEOUT'
update Configuration set [Value] = '5' WHERE [Key] = 'MAPONLINE_ONLINE_TIMEOUT'

INSERT INTO ChatMessage(ChatId, MessageIndex, MessageType, OwnerId, [Message], CreatedDate, ModifiedDate, Reads, IsDeleted)
select ChatId, MessageIndex, MessageType, OwnerId, [Message], [Time], [Time], Reads, 0 FROM ChatMessage_;

insert into Chat([Name], Members, LogoUrl) select [Name], Members, LogoUrl from Chat_;

DECLARE @cnt INT = 0;
DECLARE @msgIdx INT;

WHILE @cnt < 10
BEGIN
	SET @msgIdx = 70 + @cnt;
	INSERT INTO ChatMessage (ChatId, MessageIndex, MessageType, OwnerId, Message, Time, Reads)
	VALUES (5, @msgIdx, 2, 8, Concat('Auto ', @msgIdx), '2020-05-08 23:45:08.5875462', NULL);
    SET @cnt = @cnt + 1;
END;

delete ChatMessage where ChatId in (8)
delete Chat where Id in (8)

INSERT INTO Groups VALUES('OMCC', 'Ottawa Math and Chess Club', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, getdate(), getdate(), NULL, NULL)

ALTER TABLE GroupFriends ADD CreatedDate datetime2 null, ModifiedDate datetime2 null;
ALTER TABLE Friends ADD Address NVARCHAR(100) NULL, CountryName VARCHAR(30) NULL;
ALTER TABLE Friends ADD Info VARCHAR(255) NULL;
ALTER TABLE Friends ADD ThirdPartyLogin VARCHAR(10) NULL, ThirdPartyToken VARCHAR(255) NULL, ThirdPartyUserId VARCHAR(50) NULL;
UPDATE Friends SET ThirdPartyLogin = 'Facebook' WHERE FacebookToken IS NOT NULL;
ALTER TABLE Friends ALTER COLUMN FacebookId VARCHAR(20) NULL
Delete Friends where Name = 'Clemence Pham';
Delete Friends where Name = 'Quoc Dat PHAM';

update Friends set CountryName = N'Đức' where CountryName = 'Ð?c';
update Friends set CountryName = N'Bỉ' where CountryName = 'B?'
update Friends set CountryName = N'Việt Nam' where CountryName = 'Vi?t Nam';
update Friends set CountryName = N'Cộng hòa Séc' where CountryName = 'C?ng hòa Séc';

--Approve subscription
UPDATE GroupFriends SET Active = 1, UserRight = 3 WHERE Id = 19;
INSERT INTO Notification(Type, NotificationJson, CreatedDate, Destination, Reads, Deletions, OwnerId)
	VALUES(2, N'{"Type":2,"FriendId":4,"FriendName":"BảoAnh BảoLinh BảoChâu","FacebookId":"10217327271725297","GroupId":12,"GroupName":"A0-ĐHTH","ImageFile":"notif_SubscriptionApproved.png"}',
	GetDate(), 'u4', NULL, NULL, 1);

BEGIN
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'A0-ĐHTH')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('A0-ĐHTH', 	N'Khối chuyên toán A0-ĐHTH Hà Nội', NULL,
		N'Niên khóa', N'Lớp', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-UK')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-UK', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Vương quốc Anh',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-France')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-France', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Pháp',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-Germany')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-Germany', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Đức',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-USA')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-USA', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Mỹ',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-Australia')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-Australia', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Úc',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-Czech')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-Czech', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Cộng hòa Séc',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-Poland')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-Poland', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Ba Lan',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
IF NOT EXISTS (SELECT * FROM Groups WHERE Name = 'Hanoi9194XaXu-Russia')
	INSERT INTO Groups(Name, "Desc", Info, Cat1Desc, Cat2Desc, Cat3Desc, Cat4Desc, Cat5Desc, Cat6Desc, Cat7Desc, Cat8Desc, Cat9Desc, Active, CreatedDate, ModifiedDate, Logo)
	VALUES('Hanoi9194XaXu-Russia', 	N'Cấp 3 khóa 91-94 Hà Nội Xa xứ ở Nga',
		N'Nhóm Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.',
		N'Thành phố', N'Niên khóa', N'Nước', N'Trường', N'Lớp', NULL, NULL, NULL, NULL, 1, GETDATE(), GETDATE(), NULL);
		
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-UK'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-UK'), N'Hà nội', N'91-94', N'Vương quốc Anh',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-France'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-France'), N'Hà nội', N'91-94', N'Pháp',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Germany'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Germany'), N'Hà nội', N'91-94', N'Đức',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-USA'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-USA'), N'Hà nội', N'91-94', N'Mỹ',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Australia'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Australia'), N'Hà nội', N'91-94', N'Úc',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Czech'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Czech'), N'Hà nội', N'91-94', N'CH Séc',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Poland'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Poland'), N'Hà nội', N'91-94', N'Ba Lan',
		NULL, NULL, NULL, NULL, NULL, NULL);
IF NOT EXISTS (SELECT * FROM GroupFixedCatValues WHERE GroupId = (SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Russia'))
	INSERT INTO GroupFixedCatValues(GroupId, Cat1, Cat2, Cat3, Cat4, Cat5, Cat6, Cat7, Cat8, Cat9)
	VALUES((SELECT Id FROM Groups WHERE Name = 'Hanoi9194XaXu-Russia'), N'Hà nội', N'91-94', N'Nga',
		NULL, NULL, NULL, NULL, NULL, NULL);

IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetGroups')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetGroups', NULL, 15, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetFriend')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetFriend', NULL, 60, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetGroupFixedCatValues')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetGroupFixedCatValues', NULL, 60, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetGroupCatValues')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetGroupCatValues', NULL, 5, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetMyGroups')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetMyGroups', NULL, 5, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetGroupFriends')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetGroupFriends', NULL, 15, 1);
IF NOT EXISTS (SELECT * FROM CacheConfiguration WHERE KeyPrefix = 'goFriend.MobileAppService.Controllers.FriendController.GetNotifications')
	INSERT INTO CacheConfiguration(KeyPrefix, KeySuffixReg, Timeout, Enabled)
	VALUES('goFriend.MobileAppService.Controllers.FriendController.GetNotifications', NULL, 5, 1);

INSERT INTO GroupPredefinedCategory(GroupId, Category, ParentId)
	SELECT Id, N'K26(91-94)', NULL  FROM Groups WHERE Name = 'A0-ĐHTH';
INSERT INTO GroupPredefinedCategory(GroupId, Category, ParentId)
	SELECT Id, N'Toán A', (SELECT Id FROM GroupPredefinedCategory WHERE Name = 'A0-ĐHTH' AND Category = N'K26(91-94)')  FROM Groups WHERE Name = 'A0-ĐHTH';
INSERT INTO GroupPredefinedCategory(GroupId, Category, ParentId)
	SELECT Id, N'Toán B', (SELECT Id FROM GroupPredefinedCategory WHERE Name = 'A0-ĐHTH' AND Category = N'K26(91-94)')  FROM Groups WHERE Name = 'A0-ĐHTH';
	
INSERT INTO GroupPredefinedCategory(GroupId, Category, ParentId)
	SELECT Id, SchoolName, NULL FROM Groups,
		(SELECT N'Chuyên ĐHTH' SchoolName
		UNION SELECT N'HN Amsterdam'
		UNION SELECT N'Chu Văn An'
		UNION SELECT N'Hoàn Kiếm'
		UNION SELECT N'Kim Liên'
		UNION SELECT N'Nguyễn Trãi'
		UNION SELECT N'Quang Trung'
		UNION SELECT N'Chuyên ĐHSP'
		UNION SELECT N'Trần Phú'
		UNION SELECT N'Yên Hòa'
		UNION SELECT N'Lương Thế Vinh'
		UNION SELECT N'Chuyên ĐHNN'
		UNION SELECT N'Việt Đức'
		UNION SELECT N'Thăng Long'
		UNION SELECT N'Phan Đình Phùng'
		) Schools
	WHERE NAME LIKE '%9194%';

INSERT INTO GroupPredefinedCategory(GroupId, Category, ParentId)
	SELECT GroupId, ClassName, Id FROM GroupPredefinedCategory,
		(SELECT N'Toán A' ClassName
		UNION SELECT N'Toán B'
		) Classes
	WHERE Category = N'Chuyên ĐHTH' AND ParentId IS NULL;

END;