using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using registerition.Data;
using registerition.DTOs.Appointment;
using registerition.Models;
using System.Security.Claims;

namespace registerition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            List<Appointment> appointments;

            if (user.Role == "doctor")
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                    return NotFound("Doctor profile not found");

                appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Where(a => a.DoctorId == doctor.Id)
                    .ToListAsync();
            }
            else
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                    return NotFound("Patient profile not found");

                appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                    .Where(a => a.PatientId == patient.Id)
                    .ToListAsync();
            }

            var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.Username,
                Specialty = a.Doctor.Specialty,
                PatientName = a.Patient.User.Username,
                Date = a.AppointmentDate.ToString("MMMM d, yyyy"),
                Time = a.Time,
                Status = a.Status,
                Notes = a.Notes
            }).ToList();

            return Ok(appointmentDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found");

            // Security check - ensure user can only access their own appointments
            if (user.Role == "doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();
            }
            else
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null || appointment.PatientId != patient.Id)
                    return Forbid();
            }

            var appointmentDto = new AppointmentResponseDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.User.Username,
                Specialty = appointment.Doctor.Specialty,
                PatientName = appointment.Patient.User.Username,
                Date = appointment.AppointmentDate.ToString("MMMM d, yyyy"),
                Time = appointment.Time,
                Status = appointment.Status,
                Notes = appointment.Notes
            };

            return Ok(appointmentDto);
        }

        [HttpPost]
        [Authorize(Roles = "patient")]
        public async Task<IActionResult> CreateAppointment(AppointmentResponseDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient profile not found");

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
                return NotFound("Doctor not found");

            // Parse date string
            if (!DateTime.TryParse(dto.Date, out DateTime appointmentDate))
                return BadRequest("Invalid date format");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = patient.Id,
                AppointmentDate = appointmentDate,
                Time = dto.Time,
                Status = "pending",
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var createdAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            var appointmentDto = new AppointmentResponseDto
            {
                Id = createdAppointment!.Id,
                DoctorId = createdAppointment.DoctorId,
                DoctorName = createdAppointment.Doctor.User.Username,
                Specialty = createdAppointment.Doctor.Specialty,
                PatientName = createdAppointment.Patient.User.Username,
                Date = createdAppointment.AppointmentDate.ToString("MMMM d, yyyy"),
                Time = createdAppointment.Time,
                Status = createdAppointment.Status,
                Notes = createdAppointment.Notes
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointmentDto);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, AppointmentUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found");

            // Security check and permission validation
            if (user.Role == "doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();

                // Doctors can confirm or cancel appointments
                if (dto.Status != "confirmed" && dto.Status != "cancelled" && dto.Status != "completed")
                    return BadRequest("Invalid status");
            }
            else
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null || appointment.PatientId != patient.Id)
                    return Forbid();

                // Patients can only cancel appointments
                if (dto.Status != "cancelled")
                    return BadRequest("Patients can only cancel appointments");
            }

            appointment.Status = dto.Status;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Notes))
                appointment.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
