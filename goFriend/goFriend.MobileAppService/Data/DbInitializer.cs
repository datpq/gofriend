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
                new Group {Name = "Hanoi9194XaXu", Desc = "Cấp 3 khóa 91-94 Hà Nội Xa xứ", Cat1Desc = "Thành phố", Cat2Desc = "Niên khóa", Cat3Desc = "Trường", Cat4Desc = "Lớp"},
                new Group {Name = "Hanoi9194", Desc = "Cấp 3 khóa 91-94 Hà Nội", Cat1Desc = "Thành phố", Cat2Desc = "Niên khóa", Cat3Desc = "Trường", Cat4Desc = "Lớp"}
            };
            foreach (var x in groups)
            {
                context.Groups.Add(x);
            }
            context.SaveChanges();

            var groupCatInfos = new[]
            {
                new GroupCategory{Group = context.Groups.First(x => x.Name == "Hanoi9194XaXu"), Cat1 = "Hà nội", Cat2 = "91-94"},
                new GroupCategory{Group = context.Groups.First(x => x.Name=="Hanoi9194"), Cat1 = "Hà nội", Cat2 = "91-94"}
            };
            foreach (var x in groupCatInfos)
            {
                context.GroupCatInfos.Add(x);
            }
            context.SaveChanges();

            var friends = new[]
            {
                new Friend{Name = "DPH", FirstName = "DPH", LastName = "Phạm",
                    Email = "gofriend9194@gmail.com", Gender = "male", Birthday = new DateTime(1976, 9, 12),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new Friend{Name = "Phạm Quốc Đạt", FirstName = "Quốc Đạt", LastName = "Phạm",
                    Email = "datpq@free.fr", Gender = "male", Birthday = new DateTime(1976, 9, 12),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now},
                new Friend{Name = "Vũ Bảo Thoa", FirstName = "Bảo Thoa", LastName = "Vũ",
                    Email = "phambaothoauk@gmail.com", Gender = "female", Birthday = new DateTime(1979, 12, 3),
                    Active = true, CreatedDate = DateTime.Now, ModifiedDate = DateTime.Now}
            };
            foreach (var x in friends)
            {
                context.Friends.Add(x);
            }
            context.SaveChanges();

            var groupFriends = new[]
            {
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="gofriend9194@gmail.com"),
                    Group = context.Groups.First(x => x.Name == "Hanoi9194XaXu"), Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A"},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="gofriend9194@gmail.com"),
                    Group = context.Groups.First(x => x.Name == "Hanoi9194"), Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A"},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="datpq@free.fr"),
                    Group = context.Groups.First(x => x.Name == "Hanoi9194XaXu"), Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A"},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="datpq@free.fr"),
                    Group = context.Groups.First(x => x.Name == "Hanoi9194"), Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Chuyên ĐHTH", Cat4 = "Toán A"},
                new GroupFriend {Friend = context.Friends.First(x => x.Email=="phambaothoauk@gmail.com"),
                    Group = context.Groups.First(x => x.Name == "Hanoi9194XaXu"), Cat1 = "Hà nội", Cat2 = "91-94", Cat3 = "Amsterdam", Cat4 = "Pháp"},
            };
            foreach (var x in groupFriends)
            {
                context.GroupFriends.Add(x);
            }
            context.SaveChanges();
        }
    }
}
