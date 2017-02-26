using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Beacon.Server.Models
{
    public partial class beaconContext : DbContext
    {
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseSqlServer(@"Data Source=beacon-sql.cgxwb1ejd6bv.us-east-1.rds.amazonaws.com;Initial Catalog=beacon;Persist Security Info=True;User ID=beacon_master;Password=Beacon123!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Eid)
                    .HasName("PK__event__D9509F6D2BEB659E");

                entity.ToTable("event");

                entity.Property(e => e.Eid)
                    .HasColumnName("eid")
                    .ValueGeneratedNever();

                entity.Property(e => e.CreatorId).HasColumnName("creator_id");

                entity.Property(e => e.EDescription)
                    .HasColumnName("e_description")
                    .HasColumnType("text");

                entity.Property(e => e.EName)
                    .HasColumnName("e_name")
                    .HasColumnType("varchar(1)");

                entity.Property(e => e.TimeLastUpdated)
                    .IsRequired()
                    .HasColumnName("time_last_updated")
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.CreatorId)
                    .HasConstraintName("FK__event__creator_i__4316F928");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Uid)
                    .HasName("PK_users");

                entity.ToTable("users");

                entity.Property(e => e.Uid)
                    .HasColumnName("uid")
                    .ValueGeneratedNever();

                entity.Property(e => e.Birthdate)
                    .HasColumnName("birthdate")
                    .HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Fname)
                    .IsRequired()
                    .HasColumnName("fname")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Lname)
                    .IsRequired()
                    .HasColumnName("lname")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("varchar(20)");
            });
        }
    }
}