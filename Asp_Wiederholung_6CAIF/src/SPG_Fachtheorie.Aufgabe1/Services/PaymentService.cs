using System;
using System.Linq;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Commands;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class PaymentService
    {
        private readonly AppointmentContext _context;

        public IQueryable<Payment> Payments => _context.Payments;

        public PaymentService(AppointmentContext context)
        {
            _context = context;
        }

        public Payment CreatePayment(NewPaymentCommand cmd)
        {
            var cashDesk = _context.CashDesks.FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            var employee = _context.Employees.FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);

            if (cashDesk == null || employee == null)
                throw new PaymentServiceException("Invalid cash desk or employee.");

            var existingOpenPayment = _context.Payments
                .FirstOrDefault(p => p.CashDesk.Number == cmd.CashDeskNumber && p.Confirmed == null);

            if (existingOpenPayment != null)
                throw new PaymentServiceException("Open payment for cashdesk.");

            if (cmd.PaymentType == PaymentType.CreditCard && employee is not Manager)
                throw new PaymentServiceException("Insufficient rights to create a credit card payment.");

            var newPayment = new Payment
            {
                CashDesk = cashDesk,
                Employee = employee,
                PaymentDateTime = DateTime.UtcNow,
                Confirmed = null,
                PaymentType = cmd.PaymentType,
            };

            _context.Payments.Add(newPayment);
            _context.SaveChanges();

            return newPayment;
        }

        public void ConfirmPayment(int paymentId)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment == null)
                throw new PaymentServiceException("Payment not found.");

            if (payment.Confirmed != null)
                throw new PaymentServiceException("Payment already confirmed.");

            payment.Confirmed = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void AddPaymentItem(NewPaymentItemCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.ArticleName))
                throw new PaymentServiceException("Invalid article name.");

            if (cmd.Amount <= 0 || cmd.Price < 0)
                throw new PaymentServiceException("Invalid amount or price.");

            var payment = _context.Payments.FirstOrDefault(p => p.Id == cmd.PaymentId);
            if (payment == null)
                throw new PaymentServiceException("Payment not found.");

            if (payment.Confirmed != null)
                throw new PaymentServiceException("Payment already confirmed.");

            var item = new PaymentItem
            {
                ArticleName = cmd.ArticleName,
                Amount = cmd.Amount,
                Price = cmd.Price,
                Payment = payment
            };

            _context.PaymentItems.Add(item);
            _context.SaveChanges();
        }

        public void DeletePayment(int paymentId, bool deleteItems)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment == null)
                throw new PaymentServiceException("Payment not found.");

            if (deleteItems)
            {
                var items = _context.PaymentItems.Where(i => i.Payment.Id == paymentId);
                _context.PaymentItems.RemoveRange(items);
            }

            _context.Payments.Remove(payment);
            _context.SaveChanges();
        }
    }
}
