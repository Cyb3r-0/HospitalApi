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
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        // GET api/appointments?page=1&pageSize=10&doctorId=1&status=1&from=2025-01-01
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AppointmentQueryDto query)
        {
            var result = await _service.GetAllAsync(query);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Appointments fetched successfully",
                Data = result
            });
        }

        // GET api/appointments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var appointment = await _service.GetByIdAsync(id);
            if (appointment == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Appointment with ID {id} not found."
                });

            return Ok(new ApiResponse<AppointmentDto>
            {
                Success = true,
                Message = "Appointment fetched successfully",
                Data = appointment
            });
        }

        // POST api/appointments
        [HttpPost]
        public async Task<IActionResult> Create(AppointmentCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var (result, error) = await _service.CreateAsync(dto, userId);

            if (error != null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = error
                });

            return CreatedAtAction(nameof(Get), new { id = result!.Id },
                new ApiResponse<AppointmentDto>
                {
                    Success = true,
                    Message = "Appointment created successfully",
                    Data = result
                });
        }

        // PUT api/appointments/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, AppointmentUpdateDto dto)
        {
            var (success, error) = await _service.UpdateAsync(id, dto);

            if (!success && error == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Appointment with ID {id} not found."
                });

            if (error != null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = error
                });

            return NoContent();
        }

        // DELETE api/appointments/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Appointment with ID {id} not found."
                });

            return NoContent();
        }
    }
}