using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GroceryStore.Models
{
    public partial class GroceryStoreContext : DbContext
    {
        public GroceryStoreContext()
        {
        }

        public GroceryStoreContext(DbContextOptions<GroceryStoreContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Conversion> Conversion { get; set; }
        public virtual DbSet<Grocery> Grocery { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<ProvinceState> ProvinceState { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.ImageAlt).HasMaxLength(50);

                entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Conversion>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Grocery>(entity =>
            {
                entity.Property(e => e.ImageAlt).HasMaxLength(50);

                entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("money");

                entity.Property(e => e.Weight).HasColumnType("decimal(18, 3)");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Grocery)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Grocery_Category_FK");

                entity.HasOne(d => d.Conversion)
                    .WithMany(p => p.Grocery)
                    .HasForeignKey(d => d.ConversionId)
                    .HasConstraintName("Grocery_ConversionId_FK");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(7);

                entity.HasOne(d => d.ProvinceState)
                    .WithMany(p => p.Location)
                    .HasForeignKey(d => d.ProvinceStateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Location_ProvinceStateId");
            });

            modelBuilder.Entity<ProvinceState>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasOne(d => d.Grocery)
                    .WithMany(p => p.Stock)
                    .HasForeignKey(d => d.GroceryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Stock_GroceryId_FK");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Stock)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Stock_LocationId_FK");
            });
        }
    }
}
