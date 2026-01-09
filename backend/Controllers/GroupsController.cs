using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace SplitMate.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Zarządzanie Grupami")] // Grupowanie w Swaggerze
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetAll()
        {
            return await _context.Groups.Include(g => g.Members).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> Get(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Expenses)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();
            return group;
        }

        [HttpPost]
        public async Task<ActionResult<Group>> Create([FromBody] CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                Members = await _context.Users
                    .Where(u => dto.MemberIds.Contains(u.Id))
                    .ToListAsync()
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
        }

        // --- Zarządzanie Członkami (Nowe!) ---

        [HttpPost("{groupId}/members/{userId}")]
        public async Task<IActionResult> AddMember(int groupId, int userId)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _context.Users.FindAsync(userId);

            if (group == null || user == null) return NotFound("Grupa lub użytkownik nie istnieje.");
            if (group.Members.Any(m => m.Id == userId)) return BadRequest("Użytkownik jest już w grupie.");

            group.Members.Add(user);
            await _context.SaveChangesAsync();
            return Ok($"Użytkownik {user.Name} dodany do grupy {group.Name}");
        }

        [HttpDelete("{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int groupId, int userId)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound();

            var user = group.Members.FirstOrDefault(m => m.Id == userId);
            if (user == null) return NotFound("Użytkownika nie ma w tej grupie.");

            group.Members.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return NotFound();

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class CreateGroupDto
        {
            public string Name { get; set; } = string.Empty;
            public List<int> MemberIds { get; set; } = new();
        }
    }
}