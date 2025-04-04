using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _service;

        public PaymentsController(PaymentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<List<PaymentDto>> GetAllPayments(
            [FromQuery] int? cashDesk,
            [FromQuery] DateTime? dateFrom)
        {
            var payments = _service.Payments
                .Where(p => (!cashDesk.HasValue || p.CashDesk.Number == cashDesk.Value)
                         && (!dateFrom.HasValue || p.PaymentDateTime >= dateFrom.Value))
                .Select(p => new PaymentDto(
                    p.Id,
                    p.Employee.FirstName,
                    p.Employee.LastName,
                    p.PaymentDateTime,
                    p.CashDesk.Number,
                    p.PaymentType.ToString(),
                    p.PaymentItems.Sum(pi => pi.Price)))
                .ToList();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPaymentById(int id)
        {
            var payment = _service.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto(
                    p.Id,
                    p.Employee.FirstName,
                    p.Employee.LastName,
                    p.CashDesk.Number,
                    p.PaymentType.ToString(),
                    p.PaymentItems
                        .Select(pi => new PaymentItemDto(pi.ArticleName, pi.Amount, pi.Price))
                        .ToList()))
                .FirstOrDefault();

            if (payment is null)
                return NotFound();

            return Ok(payment);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPayment([FromBody] NewPaymentCommand cmd)
        {
            try
            {
                var payment = _service.CreatePayment(cmd);
                return CreatedAtAction(nameof(AddPayment), new { payment.Id }, payment);
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateConfirmed(int id)
        {
            try
            {
                _service.ConfirmPayment(id);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeletePayment(int id, [FromQuery] bool deleteItems)
        {
            try
            {
                _service.DeletePayment(id, deleteItems);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        // Die PUT Methode kann (wie gewünscht) auskommentiert werden:
        //[HttpPut("/api/paymentItems/{id}")]
        //public IActionResult UpdatePayment(...) { }

        [HttpPost("{id}/items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPaymentItem(int id, [FromBody] NewPaymentItemCommand cmd)
        {
            cmd.PaymentId = id; // 👈 ID aus der URL in den Command setzen
            try
            {
                _service.AddPaymentItem(cmd);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }
    }
}
