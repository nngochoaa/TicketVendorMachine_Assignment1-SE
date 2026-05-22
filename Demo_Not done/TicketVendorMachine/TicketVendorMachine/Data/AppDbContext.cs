using Microsoft.EntityFrameworkCore;
using TicketVendorMachine.Models;

namespace TicketVendorMachine.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TransportRoute> Routes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransportRoute>().HasData(
                // Bus Routes
                new TransportRoute { Id = 1, Name = "Bus 01 - Bến Thành → Bình Triệu", TransportType = "Bus", Origin = "Bến Thành", Destination = "Bình Triệu", Price = 6000, StudentPrice = 3000 },
                new TransportRoute { Id = 2, Name = "Bus 02 - Bến Thành → Bến Xe Miền Tây", TransportType = "Bus", Origin = "Bến Thành", Destination = "Bến Xe Miền Tây", Price = 6000, StudentPrice = 3000 },
                new TransportRoute { Id = 3, Name = "Bus 13 - Bến Thành → Quận 8", TransportType = "Bus", Origin = "Bến Thành", Destination = "Quận 8", Price = 7000, StudentPrice = 3500 },
                new TransportRoute { Id = 4, Name = "Bus 36 - Bến Thành → Thủ Đức", TransportType = "Bus", Origin = "Bến Thành", Destination = "Thủ Đức", Price = 8000, StudentPrice = 4000 },
                // Metro Routes
                new TransportRoute { Id = 5, Name = "Metro Line 1 - Bến Thành → Suối Tiên", TransportType = "Metro", Origin = "Bến Thành", Destination = "Suối Tiên", Price = 20000, StudentPrice = 10000 },
                new TransportRoute { Id = 6, Name = "Metro Line 1 - Bến Thành → Bình Thái", TransportType = "Metro", Origin = "Bến Thành", Destination = "Bình Thái", Price = 15000, StudentPrice = 7500 },
                new TransportRoute { Id = 7, Name = "Metro Line 1 - Thảo Điền → Suối Tiên", TransportType = "Metro", Origin = "Thảo Điền", Destination = "Suối Tiên", Price = 12000, StudentPrice = 6000 },
                // MRT Routes
                new TransportRoute { Id = 8, Name = "MRT - Quận 1 → Bình Dương", TransportType = "MRT", Origin = "Quận 1", Destination = "Bình Dương", Price = 25000, StudentPrice = 12500 },
                new TransportRoute { Id = 9, Name = "MRT - Quận 1 → Long An", TransportType = "MRT", Origin = "Quận 1", Destination = "Long An", Price = 30000, StudentPrice = 15000 }
            );
        }
    }
}
