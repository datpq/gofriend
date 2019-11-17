using System;
using System.Linq;
using goFriend.DataModel;

namespace goFriend.MobileAppService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(FriendDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Friends.Any())
            {
                return;   // DB has been seeded
            }

            var groups = new[]
            {
                new Group
                {
                    Name = "Hanoi9194XaXu", Desc = "Cấp 3 khóa 91-94 Hà Nội Xa xứ", Cat1Desc = "Thành phố", Cat2Desc = "Niên khóa", Cat3Desc = "Trường", Cat4Desc = "Lớp",
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now,
                    Info = "Group Cấp 3 khóa 91 - 94 Hà Nội Xa xứ là kênh liên lạc & kết nối của các cựu học sinh, các trường PTTH khoá 1991-1994 trên toàn Hà Nội nhưng sống ở các nước trên thế giới : Cùng nhau giao lưu, trao đổi thông tin tìm bạn cũ, bạn mới. Cùng tổ chức các hoạt động chung, chia sẻ, giúp đỡ, hợp tác.",
                    Logo = null
                },
                new Group
                {
                    Name = "Hanoi9194", Desc = "Cấp 3 khóa 91-94 Hà Nội", Cat1Desc = "Thành phố", Cat2Desc = "Niên khóa", Cat3Desc = "Trường", Cat4Desc = "Lớp",
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now,
                    Info = null,
                    Logo = null
                },
                new Group
                {
                    Name = "Amser9497", Desc = "Cấp 3 khóa 94-97 Hà Nội Amsterdam", Cat1Desc = "Trường", Cat2Desc = "Niên khóa", Cat3Desc = "Lớp", Cat4Desc = null,
                    Active = false, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now,
                    Info = null,
                    Logo = null
                },
            };
            foreach (var x in groups)
            {
                context.Groups.Add(x);
            }
            context.SaveChanges();

            var groupCatInfos = new[]
            {
                new GroupFixedCatValues{Group = context.Groups.Single(x => x.Name == "Hanoi9194XaXu"), Cat1 = "Hà nội", Cat2 = "91-94"},
                new GroupFixedCatValues{Group = context.Groups.Single(x => x.Name=="Hanoi9194"), Cat1 = "Hà nội", Cat2 = "91-94"},
                new GroupFixedCatValues{Group = context.Groups.Single(x => x.Name=="Amser9497"), Cat1 = "Amsterdam", Cat2 = "94-97"},
            };
            foreach (var x in groupCatInfos)
            {
                context.GroupFixedCatValues.Add(x);
            }
            context.SaveChanges();

            var friends = new[]
            {
                new Friend{Name = "DPH", FirstName = "DPH", LastName = "Phạm",
                    Email = "gofriend9194@gmail.com", Gender = "male", Birthday = new DateTime(1976, 9, 12),
                    FacebookId = "FakeFacebookId1", Token = Guid.NewGuid(),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new Friend{Name = "Phạm Quốc Đạt", FirstName = "Quốc Đạt", LastName = "Phạm",
                    Email = "datpq@free.fr", Gender = "male", Birthday = new DateTime(1976, 9, 12),
                    FacebookId = "FakeFacebookId2", Token = Guid.NewGuid(),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new Friend{Name = "Vũ Bảo Thoa", FirstName = "Bảo Thoa", LastName = "Vũ",
                    Email = "phambaothoauk@gmail.com", Gender = "female", Birthday = new DateTime(1979, 12, 3),
                    FacebookId = "FakeFacebookId3", Token = Guid.NewGuid(),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
            };
            foreach (var x in friends)
            {
                context.Friends.Add(x);
            }
            context.SaveChanges();

            var groupFriends = new[]
            {
                new GroupFriend {Friend = context.Friends.Single(x => x.Email=="gofriend9194@gmail.com"),
                    Group = context.Groups.Single(x => x.Name == "Hanoi9194XaXu"), Active = true, UserRight = UserType.Admin,
                    Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="gofriend9194@gmail.com"),
                    Group = context.Groups.Single(x => x.Name == "Hanoi9194"), Active = true, UserRight = UserType.Admin,
                    Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="datpq@free.fr"), UserRight = UserType.Normal,
                    Group = context.Groups.Single(x => x.Name == "Hanoi9194XaXu"), Active = true,
                    Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="datpq@free.fr"), UserRight = UserType.Normal,
                    Group = context.Groups.Single(x => x.Name == "Hanoi9194"), Active = true,
                    Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="phambaothoauk@gmail.com"), UserRight = UserType.Normal,
                    Group = context.Groups.Single(x => x.Name == "Hanoi9194XaXu"), Active = true,
                    Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Amsterdam", Cat4 = "Pháp", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="phambaothoauk@gmail.com"), UserRight = UserType.Admin,
                    Group = context.Groups.Single(x => x.Name == "Amser9497"), Active = true,
                    Cat1 = "Amsterdam", Cat2 = "94-97", Cat3 = "Pháp", CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
            };
            foreach (var x in groupFriends)
            {
                context.GroupFriends.Add(x);
            }
            context.SaveChanges();
        }
    }
}
