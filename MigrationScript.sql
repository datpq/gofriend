select * from GroupFixedCatValues;
select * from GroupFriends;
select * from Groups;
select * from Friends;
select * from GroupPredefinedCategory;
select * from CacheConfiguration;
select * from Notification;

drop table GroupFixedCatValues;
drop table GroupFriends;
drop table Friends;
drop table GroupPredefinedCategory;
drop table Groups;
drop table CacheConfiguration;
drop table Notification;

--ALTER TABLE GroupFriends ADD [Id] [int] IDENTITY(1,1) NOT NULL
--UPDATE Groups SET Active = 0 WHERE Name = 'Amser9497';

ALTER TABLE GroupFriends ADD CreatedDate datetime2 null, ModifiedDate datetime2 null;
ALTER TABLE Friends ADD Address NVARCHAR(100) NULL, CountryName VARCHAR(30) NULL;
ALTER TABLE Friends ADD Info VARCHAR(255) NULL;

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