using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class AppointmentContext : DbContext
    {
        public DbSet<Payment> Payment => Set<Payment>();
        public DbSet<PaymentItem> PaymentItem => Set<PaymentItem>();
        public DbSet<CashDesk> CashDesk => Set<CashDesk>();
        public DbSet<Cashier> Cashier => Set<Cashier>();
        public DbSet<Manager> Manager => Set<Manager>();
        public DbSet<Employee> Employee => Set<Employee>();

        public AppointmentContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Add your configuration here
            modelBuilder.Entity<Employee>().OwnsOne(e => e.Address);

            modelBuilder.Entity<Payment>().Property(p => p.PaymentType).HasConversion<string>();

            modelBuilder.Entity<Employee>().HasDiscriminator(e => e.Type);
        }
    }
}