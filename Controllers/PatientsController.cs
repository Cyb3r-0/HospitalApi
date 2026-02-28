using AutoMapper;
using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

        // GET api/patients
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PatientQueryDto query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        // GET api/patients/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var patient = await _service.GetByIdAsync(id);
            if (patient == null) return NotFound();

            return Ok(patient);
        }

        // POST api/patients
        [HttpPost]
        public async Task<IActionResult> Create(PatientCreateDto dto)
        {
            var patient = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(Get), new {id = patient.Id}, patient);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, PatientUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) NotFound();

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
