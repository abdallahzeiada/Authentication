using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using registerition.Data;
using registerition.DTOs.Doctor;
using System.Security.Claims;

namespace registerition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            var doctorDtos = doctors.Select(d => new DoctorDto
            {
                Id = d.Id,
                Name = d.User.Username,
                Specialty = d.Specialty,
                Experience = d.Experience,
                Rating = d.Rating,
                ImageUrl = d.User.ImageUrl ?? "https://images.pexels.com/photos/5452293/pexels-photo-5452293.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2",
                IsAvailable = d.IsAvailable
            }).ToList();

            return Ok(doctorDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            var doctorDto = new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.User.Username,
                Specialty = doctor.Specialty,
                Experience = doctor.Experience,
                Rating = doctor.Rating,
                ImageUrl = doctor.User.ImageUrl ?? "https://images.pexels.com/photos/5452293/pexels-photo-5452293.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2",
                IsAvailable = doctor.IsAvailable
            };

            return Ok(doctorDto);
        }

        [Authorize(Roles = "doctor")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateDoctorProfile([FromBody] DoctorDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor profile not found");

            // Update doctor info
            doctor.Specialty = dto.Specialty;
            doctor.Experience = dto.Experience;
            doctor.IsAvailable = dto.IsAvailable;

            // Update user info if needed
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                doctor.User.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
