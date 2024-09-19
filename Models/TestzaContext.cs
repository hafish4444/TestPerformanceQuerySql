using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace SQLBenchmark.Models;

public partial class TestzaContext : DbContext
{
    public TestzaContext()
    {
    }

    public TestzaContext(DbContextOptions<TestzaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    //public virtual DbSet<Paymenttransaction> Paymenttransactions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=testza;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.22-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");

            entity.ToTable("customers");

            entity.HasIndex(e => new { e.LastName, e.FirstName }, "idx_customer_name");

            entity.Property(e => e.CustomerId)
                .HasColumnType("int(11)")
                .HasColumnName("CustomerID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.CustomerId, "CustomerID");

            entity.HasIndex(e => e.OrderDate, "idx_order_date");

            entity.Property(e => e.OrderId)
                .HasColumnType("int(11)")
                .HasColumnName("OrderID");
            entity.Property(e => e.CustomerId)
                .HasColumnType("int(11)")
                .HasColumnName("CustomerID");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.ShippingAddress).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity.ToTable("orderitems");

            entity.HasIndex(e => e.OrderId, "OrderID");

            entity.HasIndex(e => e.ProductId, "ProductID");

            entity.Property(e => e.OrderItemId)
                .HasColumnType("int(11)")
                .HasColumnName("OrderItemID");
            entity.Property(e => e.OrderId)
                .HasColumnType("int(11)")
                .HasColumnName("OrderID");
            entity.Property(e => e.PriceAtTime).HasPrecision(10, 2);
            entity.Property(e => e.ProductId)
                .HasColumnType("int(11)")
                .HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasColumnType("int(11)");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("orderitems_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("orderitems_ibfk_2");
        });

        //modelBuilder.Entity<Paymenttransaction>(entity =>
        //{
        //    entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

        //    entity.ToTable("paymenttransactions");

        //    entity.HasIndex(e => e.OrderId, "OrderID");

        //    entity.Property(e => e.TransactionId)
        //        .HasColumnType("int(11)")
        //        .HasColumnName("TransactionID");
        //    entity.Property(e => e.OrderId)
        //        .HasColumnType("int(11)")
        //        .HasColumnName("OrderID");
        //    entity.Property(e => e.PaymentAmount).HasPrecision(10, 2);
        //    entity.Property(e => e.PaymentDate)
        //        .HasDefaultValueSql("current_timestamp()")
        //        .HasColumnType("timestamp");
        //    entity.Property(e => e.PaymentMethod).HasMaxLength(50);
        //    entity.Property(e => e.Status).HasMaxLength(50);

        //    entity.HasOne(d => d.Order).WithMany(p => p.Paymenttransactions)
        //        .HasForeignKey(d => d.OrderId)
        //        .OnDelete(DeleteBehavior.Cascade)
        //        .HasConstraintName("paymenttransactions_ibfk_1");
        //});

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.Price, "idx_product_price");

            entity.Property(e => e.ProductId)
                .HasColumnType("int(11)")
                .HasColumnName("ProductID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.StockQuantity).HasColumnType("int(11)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
