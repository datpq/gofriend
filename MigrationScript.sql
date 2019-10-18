select * from GroupFixedCatValues;
select * from GroupFriends;
select * from Groups;
select * from Friends;
select * from GroupPredefinedCategory;

drop table GroupFixedCatValues;
drop table GroupFriends;
drop table Friends;
drop table GroupPredefinedCategory;
drop table Groups;

--UPDATE Groups SET Active = 0 WHERE Name = 'Amser9497';

BEGIN
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