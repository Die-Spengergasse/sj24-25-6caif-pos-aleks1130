using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    public class PaymentServiceTests
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .Options;
            return new AppointmentContext(options);
        }

        [Theory]
        [InlineData(0, "Cash", 1, "Invalid cashdesk")]
        [InlineData(1, "Cash", 0, "Invalid employee")]
        [InlineData(1, "Cash", 1, "Open payment for cashdesk.")]
        [InlineData(1, "CreditCard", 1, "Insufficient rights to create a credit card payment.")]
        public void CreatePaymentExceptionsTest(int cashDeskNr, string type, int employeeNr, string expectedError)
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);

            if (cashDeskNr > 0) context.CashDesks.Add(new CashDesk(cashDeskNr));
            if (employeeNr > 0)
            {
                context.Employees.Add(new TestCashier(employeeNr, "Max", "Muster", DateOnly.FromDateTime(DateTime.Today), null, null));
                if (expectedError == "Open payment for cashdesk.")
                {
                    context.Payments.Add(new Payment(context.CashDesks.First(), DateTime.UtcNow, context.Employees.First(), PaymentType.Cash));
                }
            }

            context.SaveChanges();

            var cmd = new NewPaymentCommand(cashDeskNr, type, employeeNr);
            var ex = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.Contains(expectedError, ex.Message);
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            context.CashDesks.Add(new CashDesk(1));
            context.Employees.Add(new TestManager(1, "Max", "Muster", DateOnly.FromDateTime(DateTime.Today), 5000, null));
            context.SaveChanges();

            var cmd = new NewPaymentCommand(1, "CreditCard", 1);
            var result = service.CreatePayment(cmd);

            Assert.NotNull(result);
            Assert.Equal(1, result.CashDesk.Number);
            Assert.Equal(PaymentType.CreditCard, result.PaymentType);
        }

        [Fact]
        public void ConfirmPaymentExceptionsTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            var ex = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(1));
            Assert.Equal("Payment not found", ex.Message);

            var desk = new CashDesk(1);
            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var pay = new Payment(desk, DateTime.UtcNow, emp, PaymentType.Cash) { Confirmed = DateTime.UtcNow };
            context.CashDesks.Add(desk);
            context.Employees.Add(emp);
            context.Payments.Add(pay);
            context.SaveChanges();

            var ex2 = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(pay.Id));
            Assert.Equal("Payment already confirmed", ex2.Message);
        }

        [Fact]
        public void ConfirmPaymentSuccessTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var pay = new Payment(new CashDesk(1), DateTime.UtcNow, emp, PaymentType.Cash);
            context.CashDesks.Add(pay.CashDesk);
            context.Employees.Add(emp);
            context.Payments.Add(pay);
            context.SaveChanges();

            service.ConfirmPayment(pay.Id);
            Assert.True(context.Payments.First().Confirmed.HasValue);
        }

        [Fact]
        public void AddPaymentItemExceptionsTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            var cmd = new NewPaymentItemCommand("Item", 1, 1, 99);

            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment not found.", ex.Message);

            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var p = new Payment(new CashDesk(1), DateTime.UtcNow, emp, PaymentType.Cash) { Confirmed = DateTime.UtcNow };
            context.CashDesks.Add(p.CashDesk);
            context.Employees.Add(emp);
            context.Payments.Add(p);
            context.SaveChanges();

            var cmd2 = new NewPaymentItemCommand("Item", 1, 1, p.Id);
            var ex2 = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd2));
            Assert.Equal("Payment already confirmed.", ex2.Message);
        }

        [Fact]
        public void AddPaymentItemSuccessTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var payment = new Payment(new CashDesk(1), DateTime.UtcNow, emp, PaymentType.Cash);
            context.CashDesks.Add(payment.CashDesk);
            context.Employees.Add(emp);
            context.Payments.Add(payment);
            context.SaveChanges();

            var cmd = new NewPaymentItemCommand("Book", 1, 12.5m, payment.Id);
            service.AddPaymentItem(cmd);

            Assert.Single(context.PaymentItems);
            Assert.Equal("Book", context.PaymentItems.First().ArticleName);
        }

        [Fact]
        public void DeletePaymentExceptionsTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);

            var ex = Assert.Throws<PaymentServiceException>(() => service.DeletePayment(1, false));
            Assert.Equal("Payment not found.", ex.Message);

            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var payment = new Payment(new CashDesk(1), DateTime.UtcNow, emp, PaymentType.Cash);
            var item = new PaymentItem("Pen", 1, 2.0m, payment);

            context.CashDesks.Add(payment.CashDesk);
            context.Employees.Add(emp);
            context.Payments.Add(payment);
            context.PaymentItems.Add(item);
            context.SaveChanges();

            var ex2 = Assert.Throws<PaymentServiceException>(() => service.DeletePayment(payment.Id, false));
            Assert.Equal("Payment has payment items.", ex2.Message);
        }

        [Fact]
        public void DeletePaymentSuccessTest()
        {
            using var context = GetEmptyDbContext();
            var service = new PaymentService(context);
            var emp = new TestCashier(1, "A", "B", DateOnly.FromDateTime(DateTime.Today), null, null);
            var payment = new Payment(new CashDesk(1), DateTime.UtcNow, emp, PaymentType.Cash);
            var item = new PaymentItem("Book", 1, 10m, payment);

            context.CashDesks.Add(payment.CashDesk);
            context.Employees.Add(emp);
            context.Payments.Add(payment);
            context.PaymentItems.Add(item);
            context.SaveChanges();

            service.DeletePayment(payment.Id, true);

            Assert.Empty(context.Payments);
            Assert.Empty(context.PaymentItems);
        }

        // Testhilfe-Klassen zur Discriminator-Simulation
        private class TestCashier : Cashier
        {
            public TestCashier(int reg, string f, string l, DateOnly b, decimal? s, Address? a)
                : base(reg, f, l, b, s, a)
            {
                Type = "Cashier";
            }
        }

        private class TestManager : Manager
        {
            public TestManager(int reg, string f, string l, DateOnly b, decimal? s, Address? a)
                : base(reg, f, l, b, s, a, "DefaultCarType") // Provide a default value for 'carType'  
            {
                Type = "Manager";
            }
        }
    }
}
