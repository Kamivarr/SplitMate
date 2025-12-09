using Microsoft.AspNetCore.Mvc;
using SplitMate.Api.Data;
using SplitMate.Api.Models;

namespace SplitMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        // DTO do tworzenia użytkownika
        // ========================================================================
        public class CreateUserDto
        {
            public string Name { get; set; } = "";
        }

        // ========================================================================
        // POST /api/users
        // ========================================================================
        [HttpPost]
        public IActionResult Create([FromBody] CreateUserDto dto)
        {
            var user = new User
            {
                Name = dto.Name
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
