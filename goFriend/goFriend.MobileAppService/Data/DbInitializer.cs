using System;
using goFriend.MobileAppService.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace goFriend.MobileAppService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(FriendDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Friends.Any())
            {
                return;   // DB has been seeded
            }

            var groups = new[]
            {
                new Group {Id = 1000, Name = "Hanoi9194_XaXu", Desc = "Group for Hanoi9194 abroad"},
                new Group {Id = 1001, Name = "Hanoi9194", Desc = "Group for Hanoi9194"}
            };
            foreach (var x in groups)
            {
                context.Groups.Add(x);
            }
            var friends = new[]
            {
                new Friend{Id = 1, FirstName = "Quoc Dat", LastName = "Pham", Email = "datpquk@gmail.com", Gender = "Male", Birthday = new DateTime(1976, 9, 12) },
                new Friend{Id = 2, FirstName = "Bao Thoa", LastName = "Vu", Email = "phambaothoauk@gmail.com", Gender = "Female", Birthday = new DateTime(1979, 12, 3)}
            };
            foreach (var x in friends)
            {
                context.Friends.Add(x);
            }
            var groupFriends = new[]
            {
                new GroupFriend {Friend = context.Friends.Find(1), Group = context.Groups.Find(1000)},
                new GroupFriend {Friend = context.Friends.Find(1), Group = context.Groups.Find(1001)},
                new GroupFriend {Friend = context.Friends.Find(2), Group = context.Groups.Find(1000)},
            };
            foreach (var x in groupFriends)
            {
                context.GroupFriends.Add(x);
            }
            context.SaveChanges();
        }
    }
}
