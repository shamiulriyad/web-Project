using System;
using System.Threading.Tasks;
using Backend.API.Security;
using Backend.Application.DTOs;
using Backend.Data;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public PaymentsController(ApplicationDbContext db) => _db = db;

        [HttpPost("submit")]
        [CustomerOnly]
        public async Task<IActionResult> Submit([FromBody] SubmitPaymentRequest req)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == req.OrderId && o.UserId == req.UserId);
            if (order == null) return NotFound("Order not found");
            if (order.PaymentStatus == Backend.Domain.PaymentStatus.Paid) return BadRequest("Order already paid");

            var existing = await _db.Payments.SingleOrDefaultAsync(p => p.OrderId == req.OrderId);
            if (existing != null)
            {
                existing.TransactionId = req.TransactionId;
                existing.PaymentMethod = req.PaymentMethod;
                existing.Amount = req.Amount;
                existing.Status = Backend.Domain.PaymentStatus.Pending;
                existing.CreatedAt = DateTime.UtcNow;

                order.PaymentMethod = req.PaymentMethod;
                order.PaymentStatus = Backend.Domain.PaymentStatus.Pending;
                order.OrderStatus = Backend.Domain.OrderStatus.Pending;
                order.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Ok(existing);
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = req.OrderId,
                UserId = req.UserId,
                Amount = req.Amount,
                PaymentMethod = req.PaymentMethod,
                TransactionId = req.TransactionId,
                Status = Backend.Domain.PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Payments.AddAsync(payment);

            order.PaymentMethod = req.PaymentMethod;
            order.PaymentStatus = Backend.Domain.PaymentStatus.Pending;
            order.OrderStatus = Backend.Domain.OrderStatus.Pending;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByOrder), new { orderId = payment.OrderId }, payment);
        }

        [HttpPatch("{paymentId:guid}/verify")]
        [AdminOnly]
        public async Task<IActionResult> Verify(Guid paymentId, [FromBody] VerifyPaymentRequest req)
        {
            var payment = await _db.Payments.Include(p => p.Order).SingleOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return NotFound();

            payment.Status = req.Status;
            payment.PaymentDate = DateTime.UtcNow;

            payment.Order.PaymentStatus = req.Status;
            payment.Order.UpdatedAt = DateTime.UtcNow;

            if (req.Status == Backend.Domain.PaymentStatus.Paid)
                payment.Order.OrderStatus = Backend.Domain.OrderStatus.Confirmed;
            else if (req.Status == Backend.Domain.PaymentStatus.Failed)
                payment.Order.OrderStatus = Backend.Domain.OrderStatus.Cancelled;

            await _db.SaveChangesAsync();
            return Ok(payment);
        }

        [HttpGet("admin")]
        [AdminOnly]
        public async Task<IActionResult> AdminPayments([FromQuery] Backend.Domain.PaymentStatus? status)
        {
            var query = _db.Payments.Include(p => p.Order).AsNoTracking().AsQueryable();
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);

            var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(payments);
        }

        [HttpPut("verify")]
        [AdminOnly]
        public async Task<IActionResult> VerifyFromAdmin([FromBody] VerifyPaymentAdminRequest req)
        {
            return await Verify(req.PaymentId, new VerifyPaymentRequest(req.Status));
        }

        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var payment = await _db.Payments.AsNoTracking().SingleOrDefaultAsync(p => p.OrderId == orderId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }
    }
}
