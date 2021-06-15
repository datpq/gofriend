﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using goFriend.Services.Data;

namespace goFriend.MobileAppService.Migrations
{
    [DbContext(typeof(FriendDbContext))]
    partial class FriendDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("goFriend.DataModel.CacheConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.Property<string>("KeyPrefix")
                        .HasColumnType("VARCHAR(250)");

                    b.Property<string>("KeySuffixReg")
                        .HasColumnType("NVARCHAR(250)");

                    b.Property<int>("Timeout")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("CacheConfiguration");
                });

            modelBuilder.Entity("goFriend.DataModel.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("VARCHAR(255)");

                    b.Property<string>("Members")
                        .IsRequired()
                        .HasColumnType("VARCHAR(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(255)");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Chat");
                });

            modelBuilder.Entity("goFriend.DataModel.ChatMessage", b =>
                {
                    b.Property<int>("ChatId")
                        .HasColumnType("int");

                    b.Property<int>("MessageIndex")
                        .HasColumnType("int");

                    b.Property<string>("Attachments")
                        .HasColumnType("NVARCHAR(200)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("Message")
                        .HasColumnType("NVARCHAR(1000)");

                    b.Property<int>("MessageType")
                        .HasColumnType("int");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Reads")
                        .HasColumnType("NVARCHAR(200)");

                    b.HasKey("ChatId", "MessageIndex");

                    b.HasIndex("OwnerId");

                    b.ToTable("ChatMessage");
                });

            modelBuilder.Entity("goFriend.DataModel.Configuration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Comment")
                        .HasColumnType("VARCHAR(50)");

                    b.Property<bool>("Enabled")
                        .HasColumnType("bit");

                    b.Property<string>("Key")
                        .HasColumnType("VARCHAR(50)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("Rule")
                        .HasColumnType("NVARCHAR(500)");

                    b.Property<string>("Value")
                        .HasColumnType("VARCHAR(200)");

                    b.HasKey("Id");

                    b.ToTable("Configuration");
                });

            modelBuilder.Entity("goFriend.DataModel.Friend", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Address")
                        .HasColumnType("NVARCHAR(100)");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<string>("CountryName")
                        .HasColumnType("NVARCHAR(30)");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeviceInfo")
                        .HasColumnType("VARCHAR(170)");

                    b.Property<string>("Email")
                        .HasColumnType("VARCHAR(50)");

                    b.Property<string>("FacebookId")
                        .HasColumnType("VARCHAR(20)");

                    b.Property<string>("FacebookToken")
                        .HasColumnType("VARCHAR(400)");

                    b.Property<string>("FirstName")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Gender")
                        .HasColumnType("VARCHAR(10)");

                    b.Property<byte[]>("Image")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Info")
                        .HasColumnType("VARCHAR(255)");

                    b.Property<string>("LastName")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<Point>("Location")
                        .HasColumnType("GEOMETRY");

                    b.Property<string>("MiddleName")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR(100)");

                    b.Property<string>("Phone")
                        .HasColumnType("VARCHAR(20)");

                    b.Property<string>("Relationship")
                        .HasColumnType("VARCHAR(25)");

                    b.Property<bool?>("ShowLocation")
                        .HasColumnType("bit");

                    b.Property<string>("ThirdPartyLogin")
                        .IsRequired()
                        .HasColumnType("VARCHAR(10)");

                    b.Property<string>("ThirdPartyToken")
                        .HasColumnType("VARCHAR(400)");

                    b.Property<string>("ThirdPartyUserId")
                        .HasColumnType("VARCHAR(50)");

                    b.Property<Guid>("Token")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Friends");
                });

            modelBuilder.Entity("goFriend.DataModel.FriendLocation", b =>
                {
                    b.Property<int>("FriendId")
                        .HasColumnType("int");

                    b.Property<string>("DeviceId")
                        .HasColumnType("VARCHAR(50)");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Point>("Location")
                        .HasColumnType("GEOMETRY");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SharingInfo")
                        .HasColumnType("VARCHAR(400)");

                    b.HasKey("FriendId", "DeviceId");

                    b.ToTable("FriendLocations");
                });

            modelBuilder.Entity("goFriend.DataModel.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Cat1Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat2Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat3Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat4Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat5Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat6Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat7Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat8Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat9Desc")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Desc")
                        .HasColumnType("NVARCHAR(255)");

                    b.Property<string>("Info")
                        .HasColumnType("NVARCHAR(2000)");

                    b.Property<byte[]>("Logo")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("VARCHAR(255)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<bool>("Public")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("goFriend.DataModel.GroupFixedCatValues", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Cat1")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat2")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat3")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat4")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat5")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat6")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat7")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat8")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat9")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupFixedCatValues");
                });

            modelBuilder.Entity("goFriend.DataModel.GroupFriend", b =>
                {
                    b.Property<int>("FriendId")
                        .HasColumnType("int");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Cat1")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat2")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat3")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat4")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat5")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat6")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat7")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat8")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<string>("Cat9")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserRight")
                        .HasColumnType("int");

                    b.HasKey("FriendId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("GroupFriends");
                });

            modelBuilder.Entity("goFriend.DataModel.GroupPredefinedCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Category")
                        .HasColumnType("NVARCHAR(50)");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int?>("ParentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("ParentId");

                    b.ToTable("GroupPredefinedCategory");
                });

            modelBuilder.Entity("goFriend.DataModel.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Deletions")
                        .HasColumnType("NVARCHAR(200)");

                    b.Property<string>("Destination")
                        .HasColumnType("NVARCHAR(200)");

                    b.Property<string>("NotificationJson")
                        .HasColumnType("NVARCHAR(1000)");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Reads")
                        .HasColumnType("NVARCHAR(200)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Notification");
                });

            modelBuilder.Entity("goFriend.DataModel.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("DefaultShowLocation")
                        .HasColumnType("bit");

                    b.Property<bool>("LocationSwitch")
                        .HasColumnType("bit");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<string>("Rule")
                        .IsRequired()
                        .HasColumnType("VARCHAR(100)");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("goFriend.DataModel.Chat", b =>
                {
                    b.HasOne("goFriend.DataModel.Friend", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("goFriend.DataModel.ChatMessage", b =>
                {
                    b.HasOne("goFriend.DataModel.Chat", "Chat")
                        .WithMany()
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("goFriend.DataModel.Friend", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("goFriend.DataModel.FriendLocation", b =>
                {
                    b.HasOne("goFriend.DataModel.Friend", "Friend")
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("goFriend.DataModel.GroupFixedCatValues", b =>
                {
                    b.HasOne("goFriend.DataModel.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("goFriend.DataModel.GroupFriend", b =>
                {
                    b.HasOne("goFriend.DataModel.Friend", "Friend")
                        .WithMany()
                        .HasForeignKey("FriendId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("goFriend.DataModel.Group", "Group")
                        .WithMany("GroupFriends")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("goFriend.DataModel.GroupPredefinedCategory", b =>
                {
                    b.HasOne("goFriend.DataModel.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("goFriend.DataModel.GroupPredefinedCategory", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");
                });
#pragma warning restore 612, 618
        }
    }
}
