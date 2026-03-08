using AutoMapper;
using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Doctor")]
    [ApiController]
    [Route("api/payment-events")]
    public class PaymentEventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public PaymentEventsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET api/payment-events
        // Full event stream — all payments across all bills
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] int? billId = null, [FromQuery] int? patientId = null, [FromQuery] int? doctorId = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var query = _db.PaymentEvents.AsNoTracking().AsQueryable();

            if (billId.HasValue)
                query = query.Where(e => e.BillId == billId.Value);

            if (patientId.HasValue)
                query = query.Where(e => e.PatientId == patientId.Value);

            if (doctorId.HasValue)
                query = query.Where(e => e.DoctorId == doctorId.Value);

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderByDescending(e => e.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var mapped = _mapper.Map<List<PaymentEventDto>>(events);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Payment events fetched successfully",
                Data = new PagedResult<PaymentEventDto>
                {
                    Items = mapped,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // GET api/payment-events/bill/5
        // All payments for a specific bill — full history
        [HttpGet("bill/{billId:int}")]
        public async Task<IActionResult> GetByBill(int billId)
        {
            var events = await _db.PaymentEvents
                .AsNoTracking()
                .Where(e => e.BillId == billId)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();

            var mapped = _mapper.Map<List<PaymentEventDto>>(events);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Payment history for Bill {billId}",
                Data = mapped
            });
        }

        // GET api/payment-events/patient/3
        // All payments by a specific patient across all their bills
        [HttpGet("patient/{patientId:int}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var events = await _db.PaymentEvents
                .AsNoTracking()
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.OccurredAt)
                .ToListAsync();

            var mapped = _mapper.Map<List<PaymentEventDto>>(events);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Payment history for Patient {patientId}",
                Data = mapped
            });
        }
    }
}