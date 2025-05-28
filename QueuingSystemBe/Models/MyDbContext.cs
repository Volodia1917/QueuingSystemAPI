using Microsoft.EntityFrameworkCore;

namespace QueuingSystemBe.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Device>()
                .HasKey(e=>e.DeviceCode);
            modelBuilder.Entity<Service>()
                .HasKey(e=>e.ServiceCode);
            modelBuilder.Entity<User>()
                .HasKey(e=>e.Email);
            modelBuilder.Entity<Assignment>()
                .HasKey(e=>e.Code);
            modelBuilder.Entity<Assignment>()
                .HasOne(e=>e.Service)
                .WithMany(e=>e.Assignments)
                .HasForeignKey(e=>e.ServiceCode)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Assignment>()
                .HasOne(e => e.Device)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.DeviceCode)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
