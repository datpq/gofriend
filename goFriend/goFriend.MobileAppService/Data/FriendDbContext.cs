using goFriend.MobileAppService.Models;
using Microsoft.EntityFrameworkCore;

namespace goFriend.MobileAppService.Data
{
    public class FriendDbContext : DbContext
    {
        public FriendDbContext(DbContextOptions<FriendDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Friend>().ToTable("Friends");
            modelBuilder.Entity<GroupFriend>().ToTable("GroupFriends");
            modelBuilder.Entity<GroupFriend>().HasKey(x => new { x.FriendId, x.GroupId });
            //modelBuilder.Entity<GroupFriend>()
            //    .HasOne(x => x.Group)
            //    .WithMany(x => x.GroupFriends)
            //    .HasForeignKey(x => x.GroupId);
            //modelBuilder.Entity<GroupFriend>()
            //    .HasOne(x => x.Friend)
            //    .WithMany(x => x.GroupFriends)
            //    .HasForeignKey(x => x.FriendId);
        }

        public DbSet<Friend> Friends { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupFriend> GroupFriends { get; set; }
    }
}
