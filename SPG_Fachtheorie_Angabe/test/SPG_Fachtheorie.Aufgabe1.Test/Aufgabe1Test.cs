using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class Aufgabe1Test
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(@"Data Source=cash.db")
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        // Creates an empty DB in Debug\net8.0\cash.db
        [Fact]
        public void CreateDatabaseTest()
        {
            using var db = GetEmptyDbContext();

        }

        [Fact]
        public void AddCashierSuccessTest()
        {
            using var db = GetEmptyDbContext();
            var address = new Model.Address("s", "c", "5");
            var cashier = new Cashier(20, "F", "L", address, "as","AS");

            db.Cashier.Add(cashier);
            db.SaveChanges();

            db.ChangeTracker.Clear();
            var cashierFromDb = db.Cashier.First();
            Assert.True(cashierFromDb.JobSpezialisation == "AS");
        }

        [Fact]
        public void AddPaymentSuccessTest()
        {
            using var db = GetEmptyDbContext();
            var cashedesk = new CashDesk(5);
            var address = new Model.Address("s", "c", "5");
            var cashier = new Cashier(20, "F", "L", address, "as", "AS");
            var payment = new Payment(cashedesk, new DateTime(2025, 2, 14, 9, 0, 0), cashier);

            db.Payment.Add(payment);
            db.SaveChanges();

            db.ChangeTracker.Clear();
            var paymentFromDb = db.Payment.First();
            Assert.True(paymentFromDb.Employee == cashier);
        }

        [Fact]
        public void EmployeeDiscriminatorSuccessTest()
        {
            using var db = GetEmptyDbContext();
            var address = new Model.Address("s", "c", "5");
            var cashier = new Cashier(20, "F", "L", address, "as", "AS");
            db.Cashier.Add(cashier);
            db.SaveChanges();

            db.ChangeTracker.Clear();
            var cashierFromDb = db.Cashier.First();
            Assert.True(cashierFromDb.Type == "Employee");
        }
    }
}