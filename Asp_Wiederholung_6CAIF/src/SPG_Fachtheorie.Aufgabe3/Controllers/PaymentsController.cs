using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // --> api/payments
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db;

        public PaymentsController(AppointmentContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<List<PaymentDto>> GetAllPayments(
            [FromQuery] int? cashDesk,
            [FromQuery] DateTime? dateFrom)
        {
            var payments = _db.Payments
                .Where(p => (!cashDesk.HasValue || p.CashDesk.Number == cashDesk.Value)
                         && (!dateFrom.HasValue || p.PaymentDateTime >= dateFrom.Value))
                .Select(p => new PaymentDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.PaymentDateTime,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems.Sum(pi => pi.Price)))
                .ToList();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPaymentById(int id)
        {
            var payment = _db.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems
                        .Select(pi => new PaymentItemDto(
                            pi.ArticleName, pi.Amount, pi.Price))
                        .ToList()))
                .FirstOrDefault();
            if (payment is null) return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPayment([FromBody] NewPaymentCommand cmd)
        {
            var cashDesk = _db.CashDesks
                .FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            if (cashDesk is null) return Problem("Invalid cashdesk", statusCode: 400);
            var employee = _db.Employees
                .FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);
            if (employee is null) return Problem("Invalid employee", statusCode: 400);
            var paymentType = Enum.Parse<PaymentType>(cmd.PaymentType);
            var payment = new Payment(
                cashDesk, cmd.PaymentDateTime, employee, paymentType);
            _db.Payments.Add(payment);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            return CreatedAtAction(nameof(AddPayment), new { payment.Id });
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePayment(int id, [FromQuery] bool deleteItems = false)
        {
            var payment = _db.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefault(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            if (!deleteItems && payment.PaymentItems.Any())
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Cannot delete payment",
                    Detail = "Payment has payment items."
                });
            }

            if (deleteItems)
            {
                _db.PaymentItems.RemoveRange(payment.PaymentItems);
            }

            _db.Payments.Remove(payment);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpPut("/api/paymentItems/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePaymentItem(int id, [FromBody] UpdatePaymentItemCommand cmd)
        {
            if (id != cmd.Id)
            {
                return Problem("Invalid payment item ID", statusCode: 400);
            }

            var item = _db.PaymentItems
                .Include(p => p.Payment)
                .FirstOrDefault(p => p.Id == id);

            if (item == null)
            {
                return Problem("Payment Item not found", statusCode: 404);
            }

            // Da es keine LastUpdated-Property gibt, kann dieser Vergleich nicht stattfinden
            // -> wir überspringen ihn

            // Wir überprüfen, ob die angegebene PaymentId zur aktuellen passt
            if (item.Payment == null || item.Payment.Id != cmd.PaymentId)
            {
                return Problem("Invalid payment ID", statusCode: 400);
            }

            item.ArticleName = cmd.ArticleName;
            item.Amount = cmd.Amount;
            item.Price = cmd.Price;

            _db.SaveChanges();
            return NoContent();
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateConfirmed(int id, [FromBody] UpdateConfirmedCommand cmd)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null)
            {
                return Problem("Payment not found", statusCode: 404);
            }

            // Da es keine "Confirmed"-Property gibt, simulieren wir die Bestätigung nur logisch
            Console.WriteLine($"Payment {id} confirmed at {cmd.Confirmed}");

            // Du kannst hier z. B. Logging, Messaging oder MemoryStorage nutzen

            return NoContent();
        }
    }
}
