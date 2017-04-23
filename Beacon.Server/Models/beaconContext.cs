using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Beacon.Server.Models
{
    public partial class BeaconContext : DbContext
    {
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Token> Token { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Vote> Vote { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlServer(@"Data Source=beacon-sql.cgxwb1ejd6bv.us-east-1.rds.amazonaws.com;Initial Catalog=beacon;Persist Security Info=True;User ID=beacon_master;Password=Beacon123!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.Latitude)
                    .HasName("IX_Latitude");

                entity.HasIndex(e => e.Longitude)
                    .HasName("IX_Longitude");

                entity.Property(e => e.Deleted).HasDefaultValueSql("0");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.Latitude).HasColumnType("decimal");

                entity.Property(e => e.Longitude).HasColumnType("decimal");

                entity.Property(e => e.Name).HasColumnType("varchar(20)");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.TimeLastUpdated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("getutcdate()");

                entity.Property(e => e.VoteCount).HasDefaultValueSql("0");

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.CreatorId)
                    .HasConstraintName("CreatorID");
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.Value)
                    .HasName("PK__Token__07D9BBC3C6FA51B9");

                entity.Property(e => e.Value).HasColumnType("varchar(50)");

                entity.HasOne(d => d.CorrespondingLogin)
                    .WithMany(p => p.Token)
                    .HasForeignKey(d => d.CorrespondingLoginId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_CorrespondingLoginId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.CurrentAttendedEventId).HasColumnName("CurrentAttendedEventID");

                entity.Property(e => e.Email).HasColumnType("varchar(40)");

                entity.Property(e => e.FirstName).HasColumnType("varchar(20)");

                entity.Property(e => e.HashedPassword)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.LastName).HasColumnType("varchar(20)");

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

                entity.HasOne(d => d.CurrentAttendedEvent)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.CurrentAttendedEventId)
                    .HasConstraintName("FK_CurrentAttendedEventId");
            });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => new { e.EventId, e.UserId })
                    .HasName("PK__Vote__A83C44D4D24D79A8");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Vote)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_EventId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vote)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_UseIdr");
            });
        }
    }
}