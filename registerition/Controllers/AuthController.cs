using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using registerition.Data;
using registerition.DTOs;
using registerition.Models;
using registerition.Data;
using registerition.DTOs;
using registerition.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthExample.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("Email already registered.");

        // Create the user
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var jwtIssuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        var jwtAudience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _hasher.HashPassword(null!, dto.Password),
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Add to Doctor or Patient table based on role
        if (dto.Role == "doctor")
        {
            var doctor = new Doctor
            {
                UserId = user.Id,
                Specialty = "General", // Default value, can be updated later
                Experience = "0 years", // Default value, can be updated later
                Rating = 0, // Default value
                IsAvailable = true // Default value
            };
            _context.Doctors.Add(doctor);
        }
        else if (dto.Role == "patient")
        {
            var patient = new Patient
            {
                UserId = user.Id,
                DateOfBirth = DateTime.UtcNow, // Default value, can be updated later
                MedicalHistory = null // Default value
            };
            _context.Patients.Add(patient);
        }

        await _context.SaveChangesAsync();

        return Ok("Registered successfully.");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result != PasswordVerificationResult.Success)
            return Unauthorized("Invalid credentials.");

        var token = GenerateToken(user);
        return Ok(new { token });
    }

    [HttpGet("me")]
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            name = user.Username,
            email = user.Email,
            userType = user.Role ?? "patient"
        });
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name, user.Username),
               new Claim(ClaimTypes.Role, user.Role)
           };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
