using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Beacon.Server.Models;

namespace Beacon.Server.Migrations
{
    [DbContext(typeof(BeaconContext))]
    partial class beaconContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Beacon.Server.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CreatorId");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime?>("TimeLastUpdated")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Event");
                });

            modelBuilder.Entity("Beacon.Server.Models.Token", b =>
                {
                    b.Property<string>("Value")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Value");

                    b.ToTable("Token");
                });

            modelBuilder.Entity("Beacon.Server.Models.User", b =>
                {
                    b.Property<int>("Id");

                    b.Property<int?>("CurrentAttendedEventId")
                        .HasColumnName("CurrentAttendedEventID");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("CurrentAttendedEventId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Beacon.Server.Models.Vote", b =>
                {
                    b.Property<int>("EventId");

                    b.Property<int>("UserId");

                    b.Property<int>("NumVotes");

                    b.HasKey("EventId", "UserId")
                        .HasName("PK__Vote__A83C44D4D24D79A8");

                    b.HasIndex("UserId");

                    b.ToTable("Vote");
                });

            modelBuilder.Entity("Beacon.Server.Models.Event", b =>
                {
                    b.HasOne("Beacon.Server.Models.User", "Creator")
                        .WithMany("Event")
                        .HasForeignKey("CreatorId")
                        .HasConstraintName("CreatorID");
                });

            modelBuilder.Entity("Beacon.Server.Models.User", b =>
                {
                    b.HasOne("Beacon.Server.Models.Event", "CurrentAttendedEvent")
                        .WithMany("User")
                        .HasForeignKey("CurrentAttendedEventId")
                        .HasConstraintName("FK_CurrentAttendedEventId");
                });

            modelBuilder.Entity("Beacon.Server.Models.Vote", b =>
                {
                    b.HasOne("Beacon.Server.Models.Event", "Event")
                        .WithMany("Vote")
                        .HasForeignKey("EventId")
                        .HasConstraintName("FK_EventId");

                    b.HasOne("Beacon.Server.Models.User", "User")
                        .WithMany("Vote")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UseIdr");
                });
        }
    }
}
