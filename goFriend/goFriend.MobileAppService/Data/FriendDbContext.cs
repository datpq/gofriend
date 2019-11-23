using goFriend.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace goFriend.MobileAppService.Data
{
    public class FriendDbContext : DbContext
    {
        public FriendDbContext(DbContextOptions<FriendDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>().ToTable("Groups").HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Group>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Friend>().ToTable("Friends").HasIndex(x => x.FacebookId).IsUnique();
            modelBuilder.Entity<Friend>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<GroupFixedCatValues>().ToTable("GroupFixedCatValues");
            modelBuilder.Entity<GroupFixedCatValues>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<GroupFriend>().ToTable("GroupFriends");
            modelBuilder.Entity<GroupFriend>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<GroupFriend>().HasKey(x => new { x.FriendId, x.GroupId });
            modelBuilder.Entity<GroupFriend>()
                .HasOne(x => x.Group)
                .WithMany(x => x.GroupFriends)
                .HasForeignKey(x => x.GroupId);
            modelBuilder.Entity<GroupFriend>()
                .HasOne(x => x.Friend)
                .WithMany(x => x.GroupFriends)
                .HasForeignKey(x => x.FriendId);
            modelBuilder.Entity<GroupFriend>().Property(x => x.UserRight)
                .HasConversion(new EnumToNumberConverter<UserType, int>());
            modelBuilder.Entity<GroupPredefinedCategory>().ToTable("GroupPredefinedCategory");
            modelBuilder.Entity<GroupPredefinedCategory>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CacheConfiguration>().ToTable("CacheConfiguration");
            modelBuilder.Entity<CacheConfiguration>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<Notification>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Notification>().Property(x => x.Type)
                .HasConversion(new EnumToNumberConverter<NotificationType, int>());
        }

        public DbSet<Friend> Friends { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupFriend> GroupFriends { get; set; }
        public DbSet<GroupFixedCatValues> GroupFixedCatValues { get; set; }
        public DbSet<CacheConfiguration> CacheConfigurations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        //public DbSet<NotifNewSubscriptionRequest> NotifNewSubscriptionRequests { get; set; }
        //public DbSet<NotifSubscriptionApproved> NotifSubscriptionApproveds { get; set; }
    }
}
