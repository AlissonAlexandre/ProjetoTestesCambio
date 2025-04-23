using Microsoft.EntityFrameworkCore;
using CambioAPI.Models;

namespace CambioAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerLimit> CustomerLimits { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ExchangeOperation> ExchangeOperations { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Document)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<CustomerDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });

            
            modelBuilder.Entity<CustomerLimit>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Limit)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastUpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.LastUpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });

            
            modelBuilder.Entity<Currency>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Currency>()
                .Property(c => c.ExchangeRate)
                .HasPrecision(18, 4);

            
            modelBuilder.Entity<ExchangeOperation>()
                .HasOne(eo => eo.Customer)
                .WithMany(c => c.ExchangeOperations)
                .HasForeignKey(eo => eo.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExchangeOperation>()
                .HasOne(eo => eo.FromCurrency)
                .WithMany(c => c.FromOperations)
                .HasForeignKey(eo => eo.FromCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExchangeOperation>()
                .HasOne(eo => eo.ToCurrency)
                .WithMany(c => c.ToOperations)
                .HasForeignKey(eo => eo.ToCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExchangeOperation>()
                .HasOne(eo => eo.CreatedByUser)
                .WithMany()
                .HasForeignKey(eo => eo.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExchangeOperation>()
                .Property(eo => eo.Amount)
                .HasPrecision(18, 4);

            modelBuilder.Entity<ExchangeOperation>()
                .Property(eo => eo.ExchangeRate)
                .HasPrecision(18, 4);

            modelBuilder.Entity<ExchangeOperation>()
                .Property(eo => eo.FinalAmount)
                .HasPrecision(18, 4);
        }
    }
} 