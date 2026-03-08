using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalApi.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _service;

        public BillsController(IBillService service)
        {
            _service = service;
        }

        // GET api/bills?page=1&pageSize=10&patientId=1&paymentStatus=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BillQueryDto query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Bills fetched successfully",
                Data = result
            });
        }

        // GET api/bills/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var bill = await _service.GetByIdAsync(id);
            if (bill == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Bill with ID {id} not found."
                });

            return Ok(new ApiResponse<BillDto>
            {
                Success = true,
                Message = "Bill fetched successfully",
                Data = bill
            });
        }

        // POST api/bills
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(BillCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var (result, error) = await _service.CreateAsync(dto, userId);

            if (error != null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = error });

            return CreatedAtAction(nameof(Get), new { id = result!.Id },
                new ApiResponse<BillDto>
                {
                    Success = true,
                    Message = "Bill created successfully",
                    Data = result
                });
        }

        // PUT api/bills/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, BillUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var (success, error) = await _service.UpdateAsync(id, dto, userId);

            if (!success && error == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"Bill with ID {id} not found." });

            if (error != null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = error });

            return NoContent();
        }

        // POST api/bills/5/pay  ← dedicated pay endpoint
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("{id:int}/pay")]
        public async Task<IActionResult> Pay(int id, BillPayDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var (success, error) = await _service.PayAsync(id, dto, userId);

            if (!success && error == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"Bill with ID {id} not found." });

            if (error != null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = error });

            var updated = await _service.GetByIdAsync(id);
            return Ok(new ApiResponse<BillDto>
            {
                Success = true,
                Message = "Payment recorded successfully",
                Data = updated
            });
        }

        // DELETE api/bills/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<object> { Success = false, Message = $"Bill with ID {id} not found." });

            return NoContent();
        }
    }
}