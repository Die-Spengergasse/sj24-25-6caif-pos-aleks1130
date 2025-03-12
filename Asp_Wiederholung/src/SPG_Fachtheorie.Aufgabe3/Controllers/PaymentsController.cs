using Microsoft.AspNetCore.Mvc;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] NewPaymentCommand command)
    {
        if (command.PaymentDateTime > DateTime.UtcNow.AddMinutes(1))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid payment date", Detail = "Payment date cannot be more than 1 minute in the future." });
        }

        var cashDesk = _context.CashDesks.FirstOrDefault(cd => cd.Number == command.CashDeskNumber);
        if (cashDesk == null)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid cash desk", Detail = "Cash desk not found." });
        }

        var employee = _context.Employees.FirstOrDefault(e => e.RegistrationNumber == command.EmployeeRegistrationNumber);
        if (employee == null)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid employee", Detail = "Employee not found." });
        }

        if (!Enum.TryParse<PaymentType>(command.PaymentType, out var paymentType))
        {
            return BadRequest(new ProblemDetails { Title = "Invalid payment type", Detail = "Payment type not recognized." });
        }

        var payment = new Payment
        {
            CashDeskId = cashDesk.Id,
            EmployeeId = employee.Id,
            PaymentDateTime = command.PaymentDateTime,
            PaymentType = paymentType
        };

        try
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ProblemDetails { Title = "Database error", Detail = ex.Message });
        }

        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment.Id);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        return Ok(payment);
    }
}
