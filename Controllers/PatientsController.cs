using HospitalApi.Dtos;
using HospitalApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalApi.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientsController(IPatientService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PatientQueryDto query)
        {
            var result = await _service.GetAllAsync(query);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Patients fetched successfully",
                Data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var patient = await _service.GetByIdAsync(id);
            if (patient == null)
                return NotFound();

            return Ok(new ApiResponse<PatientDto>
            {
                Success = true,
                Message = "Patient fetched successfully",
                Data = patient
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PatientCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var patient = await _service.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(Get), new { id = patient.Id },
                new ApiResponse<PatientDto>
                {
                    Success = true,
                    Message = "Patient created successfully",
                    Data = patient
                });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, PatientUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();

            return NoContent(); //204
        }

        // Delete api/patient/{id}
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}