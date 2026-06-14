using IMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Inventory> Inventory => Set<Inventory>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseProduct> PurchaseProducts => Set<PurchaseProduct>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleProducts> SaleProducts => Set<SaleProducts>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Customer>().ToTable("Customers");

            builder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasKey(x => x.ProductId);
                entity.HasOne(x => x.Product)
                    .WithOne()
                    .HasForeignKey<Inventory>(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(x => x.AverageCost).HasPrecision(18, 2);
            });

            builder.Entity<InventoryTransaction>(entity =>
            {
                entity.ToTable("InventoryTransactions");
                entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.Property(x => x.CostPrice).HasPrecision(18, 2);
                entity.Property(x => x.SalePrice).HasPrecision(18, 2);
            });

            builder.Entity<ProductSupplier>(entity =>
            {
                entity.ToTable("ProductSuppliers");
                entity.HasKey(x => new { x.ProductId, x.SupplierId });
                entity.Property(x => x.CostPrice).HasPrecision(18, 2);
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Supplier)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Purchase>(entity =>
            {
                entity.ToTable("Purchases");
                entity.Property(x => x.TotalExpectedAmount).HasPrecision(18, 2);
                entity.Property(x => x.TotalPaidAmount).HasPrecision(18, 2);
                entity.Ignore(x => x.DiscountAmount);
                entity.HasOne(x => x.Supplier)
                    .WithMany()
                    .HasForeignKey(x => x.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.PurchaseProducts)
                    .WithOne(x => x.Purchase)
                    .HasForeignKey(x => x.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PurchaseProduct>(entity =>
            {
                entity.ToTable("PurchaseProducts");
                entity.Property(x => x.CostPrice).HasPrecision(18, 2);
                entity.Ignore(x => x.LineTotal);
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Sale>(entity =>
            {
                entity.ToTable("Sales");
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
                entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
                entity.Property(x => x.Profit).HasPrecision(18, 2);
                entity.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.SaleProducts)
                    .WithOne(x => x.Sale)
                    .HasForeignKey(x => x.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SaleProducts>(entity =>
            {
                entity.ToTable("SaleProducts");
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.Property(x => x.TotalPrice).HasPrecision(18, 2);
                entity.Property(x => x.Profit).HasPrecision(18, 2);
                entity.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Supplier>().ToTable("Suppliers");
        }
    }
}
