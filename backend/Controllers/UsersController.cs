using Microsoft.AspNetCore.Mvc;
using SplitMate.Api.Data;
using SplitMate.Api.Models;
using SplitMate.Api.Services; // Dodano namespace dla AuthService
using Microsoft.AspNetCore.Authorization;

namespace SplitMate.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Zarządzanie Użytkownikami")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================================================
        // GET /api/users
        // ========================================================================
        [HttpGet]
        public IActionResult GetAll() => Ok(_context.Users.ToList());

        // ========================================================================
        // GET /api/users/{id}
        // ========================================================================
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // ========================================================================
        // DTO do tworzenia użytkownika (Rejestracja przez admina/API)
        // ========================================================================
        public class CreateUserDto
        {
            public string Name { get; set; } = string.Empty;
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        // ========================================================================
        // POST /api/users - Pełna rejestracja z hashowaniem hasła
        // ========================================================================
        [AllowAnonymous] // Opcjonalnie: odkomentuj, jeśli chcesz pozwolić na rejestrację bez logowania
        [HttpPost]
        public IActionResult Create([FromBody] CreateUserDto dto)
        {
            // 1. Walidacja danych wejściowych
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Login i hasło są wymagane.");

            // 2. Sprawdzenie unikalności loginu
            if (_context.Users.Any(u => u.Login.ToLower() == dto.Login.ToLower()))
                return BadRequest("Taki login jest już zajęty.");

            // 3. Hashowanie hasła (zgodnie z logiką AuthService)
            AuthService.CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // 4. Tworzenie encji użytkownika
            var user = new User
            {
                Name = dto.Name,
                Login = dto.Login,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // ========================================================================
        // DTO do aktualizacji użytkownika
        // ========================================================================
        public class UpdateUserDto
        {
            public string Name { get; set; } = "";
        }

        // ========================================================================
        // PUT /api/users/{id}
        // ========================================================================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            _context.SaveChanges();
            return NoContent();
        }

        // ========================================================================
        // DELETE /api/users/{id}
        // ========================================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }
    }
}