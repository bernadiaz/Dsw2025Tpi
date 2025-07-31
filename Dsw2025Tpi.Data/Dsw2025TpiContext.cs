using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>(eb =>
        {
            eb.ToTable("Customers");
            eb.HasKey(c => c.Id);
            eb.Property(c => c.Name).HasMaxLength(50).IsRequired();
            eb.Property(c => c.Email).HasMaxLength(100).IsRequired();
            eb.Property(c => c.PhoneNumber).HasMaxLength(15).IsRequired();
        });
        modelBuilder.Entity<Product>(eb =>
        {
            eb.ToTable("Products");
            eb.HasKey(p => p.Id);
            eb.Property(p => p.Sku).HasMaxLength(20).IsRequired();
            eb.Property(p => p.InternalCode).HasMaxLength(30).IsRequired();
            eb.Property(p => p.Name).HasMaxLength(50).IsRequired();
            eb.Property(p => p.Description).HasMaxLength(250);
            eb.Property(p => p.CurrentUnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            eb.Property(p => p.StockQuantity).IsRequired();
            eb.Property(p => p.IsActive).IsRequired();
        });
        modelBuilder.Entity<Order>(eb =>
        {
            eb.ToTable("Orders");
            eb.HasKey(o => o.Id);
            eb.Property(o => o.ShippingAddress).HasMaxLength(150).IsRequired();
            eb.Property(o => o.BillingAddress).HasMaxLength(150).IsRequired();
            eb.Property(o => o.CustomerId).IsRequired();
            eb.Property(o => o.DateTime).IsRequired();
            eb.Property(o => o.Notes).HasMaxLength(500);

        });
        modelBuilder.Entity<OrderItem>(eb =>
        {
            eb.ToTable("OrderItems");
            eb.HasKey(oi => oi.Id);
            eb.Property(oi => oi.Quantity).IsRequired();
            eb.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            eb.Property(oi => oi.ProductId).IsRequired();
            eb.Property(oi => oi.OrderId).IsRequired();
        });
    }
}