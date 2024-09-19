using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SQLBenchmark.Models;
using SQLBenchmark.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace SQLBenchmarkORM
{
    // Scaffolded DbContext
    public class TestzaContext : DbContext
    {
        // Constructor that accepts DbContextOptions
        public TestzaContext(DbContextOptions<TestzaContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Orderitem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        //public DbSet<Paymenttransaction> Paymenttransactions { get; set; }
    }

    public class SqlOrmBenchmarks
    {
        private TestzaContext _context;

        // Global setup for the benchmark - initializes the DbContext
        [GlobalSetup]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TestzaContext>()
                .UseMySql("Server=localhost;Database=testza;User=root;Password=;",
                          new MySqlServerVersion(new Version(8, 0, 21)))
                .Options;
            _context = new TestzaContext(options);
            _context.Database.OpenConnection();  // Optionally open the connection manually
        }

        // Global cleanup to properly dispose of the DbContext
        [GlobalCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        // Benchmark method for a simple ORM query
        //[Benchmark]
        //public void BenchmarkSimpleOrmQuery()
        //{
        //    try
        //    {
        //        var orders = _context.Orders.Take(1000).ToList();
        //        var list = orders.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error during query: {ex.Message}");
        //    }
        //}

        // Benchmark method for a more complex ORM query
        [Benchmark]
        public void OrmQuerySplitQuery()
        {
            try
            {
                var query = _context.Customers
                    .Select(g => new CustomerOrder
                    {
                        CustomerId = g.CustomerId,
                        CustomerName = g.FirstName + " " + g.LastName,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        Orders = g.Orders.Select(o => new CustomerOrder.Order
                        {
                            OrderId = o.OrderId,
                            ShippingAddress = o.ShippingAddress,
                            OrderDate = o.OrderDate,
                            Status = o.Status,
                            TotalAmount = o.TotalAmount,
                            TotalOrder = o.Orderitems.Count,
                            TotalOrderValue = o.Orderitems.Sum(s => s.Quantity * s.PriceAtTime)
                        })
                        .ToList()
                    })
                    .AsSplitQuery();

                var list = query.Take(1000).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during complex query: {ex.Message}");
            }
        }
        
        [Benchmark]
        public void OrmQuery()
        {
            try
            {
                var query = _context.Customers
                    .Select(g => new CustomerOrder
                    {
                        CustomerId = g.CustomerId,
                        CustomerName = g.FirstName + " " + g.LastName,
                        Email = g.Email,
                        PhoneNumber = g.PhoneNumber,
                        Orders = g.Orders.Select(o => new CustomerOrder.Order
                        {
                            OrderId = o.OrderId,
                            ShippingAddress = o.ShippingAddress,
                            OrderDate = o.OrderDate,
                            Status = o.Status,
                            TotalAmount = o.TotalAmount,
                            TotalOrder = o.Orderitems.Count,
                            TotalOrderValue = o.Orderitems.Sum(s => s.Quantity * s.PriceAtTime)
                        }).ToList()
                    });

                var list = query.Take(1000).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during complex query: {ex.Message}");
            }
        }
    
        [Benchmark]
        public void OrmLinQQuery()
        {
            try
            {
                var query = from customer in _context.Customers
                            join order in _context.Orders on customer.CustomerId equals order.CustomerId
                            join orderItem in _context.OrderItems on order.OrderId equals orderItem.OrderId into orderItemsGroup
                            select new
                            {
                                CustomerId = customer.CustomerId,
                                CustomerName = customer.FirstName + " " + customer.LastName,
                                Email = customer.Email,
                                PhoneNumber = customer.PhoneNumber,
                                Orders = new
                                {
                                    OrderId = order.OrderId,
                                    ShippingAddress = order.ShippingAddress,
                                    OrderDate = order.OrderDate,
                                    Status = order.Status,
                                    TotalAmount = order.TotalAmount,
                                    TotalOrder = orderItemsGroup.Count(), // Count of order items
                                    TotalOrderValue = orderItemsGroup.Sum(oi => oi.Quantity * oi.PriceAtTime) // Sum of order items' value
                                },
                                TotalOrdersCount = customer.Orders.Count
                            }
                            into result
                            orderby result.TotalOrdersCount descending
                            // Group by customer properties
                            group result by new
                            {
                                result.CustomerId,
                                result.CustomerName,
                                result.Email,
                                result.PhoneNumber
                            } into g
                            select new CustomerOrder
                            {
                                CustomerId = g.Key.CustomerId,
                                CustomerName = g.Key.CustomerName,
                                Email = g.Key.Email,
                                PhoneNumber = g.Key.PhoneNumber,
                                Orders = g.Select(x => new CustomerOrder.Order
                                {
                                    OrderId = x.Orders.OrderId,
                                    ShippingAddress = x.Orders.ShippingAddress,
                                    OrderDate = x.Orders.OrderDate,
                                    Status = x.Orders.Status,
                                    TotalAmount = x.Orders.TotalAmount,
                                    TotalOrder = x.Orders.TotalOrder,
                                    TotalOrderValue = x.Orders.TotalOrderValue
                                }).ToList()
                            };

                var resultList = query.Take(1000).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during complex query: {ex.Message}");
            }
        }

        [Benchmark]
        public void OrmQueryWithRawSql()
        {
            try
            {
                var sqlQuery = @"
                    SELECT 
                        c.CustomerId,
                        CONCAT(c.FirstName, ' ', c.LastName) AS CustomerName,
                        c.Email,
                        c.PhoneNumber,
                        o.OrderId,
                        o.OrderDate,
                        o.TotalAmount,
                        o.ShippingAddress,
                        o.Status,
                        COUNT(oi.OrderItemId) AS TotalOrder,
                        SUM(oi.Quantity * oi.PriceAtTime) AS TotalOrderValue
                    FROM 
                        Customers c
                    JOIN 
                        Orders o ON c.CustomerId = o.CustomerId
                    JOIN 
                        OrderItems oi ON o.OrderId = oi.OrderId
                    GROUP BY 
                        c.CustomerId, 
                        c.FirstName, 
                        c.LastName, 
                        c.Email, 
                        c.PhoneNumber, 
                        o.OrderId, 
                        o.OrderDate, 
                        o.ShippingAddress, 
                        o.Status, 
                        o.TotalAmount
                    ORDER BY 
                        TotalOrder DESC";

                // Execute the raw SQL query
                var customerOrders = new List<CustomerOrder>();

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    _context.Database.OpenConnection();

                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            // Check if customer already exists in list
                            var customerId = result.GetInt32(result.GetOrdinal("CustomerId"));
                            var customer = customerOrders.FirstOrDefault(c => c.CustomerId == customerId);
                    
                            if (customer == null)
                            {
                                customer = new CustomerOrder
                                {
                                    CustomerId = customerId,
                                    CustomerName = result.GetString(result.GetOrdinal("CustomerName")),
                                    Email = result.IsDBNull(result.GetOrdinal("Email")) ? null : result.GetString(result.GetOrdinal("Email")),
                                    PhoneNumber = result.IsDBNull(result.GetOrdinal("PhoneNumber")) ? null : result.GetString(result.GetOrdinal("PhoneNumber")),
                                    Orders = new List<CustomerOrder.Order>()
                                };

                                customerOrders.Add(customer);
                            }

                            // Add order to the customer's orders list
                            customer.Orders.Add(new CustomerOrder.Order
                            {
                                OrderId = result.GetInt32(result.GetOrdinal("OrderId")),
                                OrderDate = result.GetDateTime(result.GetOrdinal("OrderDate")),
                                TotalAmount = result.IsDBNull(result.GetOrdinal("TotalAmount")) ? (decimal?)null : result.GetDecimal(result.GetOrdinal("TotalAmount")),
                                ShippingAddress = result.IsDBNull(result.GetOrdinal("ShippingAddress")) ? null : result.GetString(result.GetOrdinal("ShippingAddress")),
                                Status = result.IsDBNull(result.GetOrdinal("Status")) ? null : result.GetString(result.GetOrdinal("Status")),
                                TotalOrder = result.IsDBNull(result.GetOrdinal("TotalOrder")) ? (int?)null : result.GetInt32(result.GetOrdinal("TotalOrder")),
                                TotalOrderValue = result.IsDBNull(result.GetOrdinal("TotalOrderValue")) ? (decimal?)null : result.GetDecimal(result.GetOrdinal("TotalOrderValue"))
                            });
                        }
                    }
                }
                Console.WriteLine(customerOrders);

                // Use your result list as needed
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during complex query: {ex.Message}");
            }
        }
        
        [Benchmark]
        public void FetchCustomerOrdersFromJson()
        {
             var query = @"
                SELECT 
                    JSON_OBJECT(
                        'CustomerId', CustomerId,
                        'CustomerName', CONCAT(FirstName, ' ', LastName),
                        'Email', Email,
                        'PhoneNumber', PhoneNumber,
                        'Orders', (
                            SELECT 
                                CONCAT('[', GROUP_CONCAT(
                                    JSON_OBJECT(
                                        'OrderId', OrderId,
                                        'OrderDate', OrderDate,
                                        'TotalAmount', TotalAmount,
                                        'ShippingAddress', ShippingAddress,
                                        'Status', Status,
                                        'TotalOrder', (
                                            SELECT COUNT(*)
                                            FROM OrderItems
                                            WHERE OrderItems.OrderId = Orders.OrderId
                                        ),
                                        'TotalOrderValue', (
                                            SELECT SUM(Quantity * PriceAtTime)
                                            FROM OrderItems
                                            WHERE OrderItems.OrderId = Orders.OrderId
                                        )
                                    )
                                ), ']')
                            FROM Orders
                            WHERE Orders.CustomerId = Customers.CustomerId
                        )
                    ) AS CustomerOrder
                FROM Customers;
            ";

            try
            {
                _context.Database.OpenConnection(); // Open the connection synchronously

                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = System.Data.CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var jsonResult = reader.GetString(0); // Assuming JSON is in the first column

                            if (jsonResult != null)
                            {
                                // Deserialize JSON to List<CustomerOrder>
                                var customerOrders = JsonConvert.DeserializeObject<List<CustomerOrder>>(jsonResult);

                                // Process customerOrders as needed
                            }
                        }
                        else
                        {
                            Console.WriteLine("No data returned.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection(); // Ensure the connection is closed
            }
        }
    }
    

    class Program
    {
        static void Main(string[] args)
        {
            // Running benchmarks directly with BenchmarkDotNet
            //var config = ManualConfig
            //        .Create(DefaultConfig.Instance)
            //        .WithOptions(ConfigOptions.JoinSummary)
            //        .WithOptions(ConfigOptions.DontOverwriteResults)
            //        .WithOptions(ConfigOptions.KeepBenchmarkFiles);
            //var summary = BenchmarkRunner.Run<SqlOrmBenchmarks>(config);

            // Alternatively, manually running the benchmark queries for testing purposes
            var sqlOrm = new SqlOrmBenchmarks();
            sqlOrm.Setup();  // Manually set up the DbContext
            sqlOrm.OrmQuerySplitQuery();
            sqlOrm.Cleanup();  // Clean up the DbContext


        }
    }
}
