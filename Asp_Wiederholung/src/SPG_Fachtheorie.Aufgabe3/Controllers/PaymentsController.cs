using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentContext _context;

        public PaymentsController(PaymentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PaymentDto>> GetPayments([FromQuery] int? cashDesk, [FromQuery] DateTime? dateFrom)
        {
            var paymentsQuery = _context.Payments.AsQueryable();

            if (cashDesk.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.CashDesk.Number == cashDesk.Value);
            }

            if (dateFrom.HasValue)
            {
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDateTime >= dateFrom.Value);
            }

            var payments = paymentsQuery.Select(p => new PaymentDto
            {
                Id = p.Id,
                EmployeeFirstName = p.Employee.FirstName,
                EmployeeLastName = p.Employee.LastName,
                CashDeskNumber = p.CashDesk.Number,
                PaymentType = p.PaymentType.ToString(),
                TotalAmount = p.PaymentItems.Sum(i => i.Amount * i.Price)
            }).ToList();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPaymentById(int id)
        {
            var payment = _context.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto
                {
                    Id = p.Id,
                    EmployeeFirstName = p.Employee.FirstName,
                    EmployeeLastName = p.Employee.LastName,
                    CashDeskNumber = p.CashDesk.Number,
                    PaymentType = p.PaymentType.ToString(),
                    PaymentItems = p.PaymentItems.Select(i => new PaymentItemDto
                    {
                        ArticleName = i.ArticleName,
                        Amount = i.Amount,
                        Price = i.Price
                    }).ToList()
                }).FirstOrDefault();

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }
    }