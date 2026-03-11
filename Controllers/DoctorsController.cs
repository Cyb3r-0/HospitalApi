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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _service;

        public DoctorsController(IDoctorService service)
        {
            _service = service;
        }

        // GET api/doctors?page=1&pageSize=10&specialization=cardiology&isAvailable=true
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DoctorQueryDto query)
        {
            var result = await _service.GetAllAsync(query);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Doctors fetched successfully",
                Data = result
            });
        }

        // GET api/doctors/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var doctor = await _service.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Doctor with ID {id} not found."
                });

            return Ok(new ApiResponse<DoctorDto>
            {
                Success = true,
                Message = "Doctor fetched successfully",
                Data = doctor
            });
        }

        // POST api/doctors
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(DoctorCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var doctor = await _service.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(Get), new { id = doctor.Id },
                new ApiResponse<DoctorDto>
                {
                    Success = true,
                    Message = "Doctor created successfully",
                    Data = doctor
                });
        }

        // PUT api/doctors/5
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, DoctorUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Doctor with ID {id} not found."
                });

            return NoContent();
        }

        // DELETE api/doctors/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Doctor with ID {id} not found."
                });

            return NoContent();
        }
    }
}