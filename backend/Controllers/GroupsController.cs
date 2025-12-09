using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;

namespace SplitMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupsController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================================================
        // GET /api/groups
        // ========================================================================
        [HttpGet]
        public IActionResult GetAll()
        {
            var groups = _context.Groups
                .Include(g => g.Members)
                .ToList();
            return Ok(groups);
        }

        // ========================================================================
        // GET /api/groups/{id}
        // ========================================================================
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var group = _context.Groups
                .Include(g => g.Members)
                .FirstOrDefault(g => g.Id == id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        // ========================================================================
        // DTO do tworzenia grupy
        // ========================================================================
        public class CreateGroupDto
        {
            public string Name { get; set; } = "";
            public List<int> MemberIds { get; set; } = new();
        }

        // ========================================================================
        // POST /api/groups
        // ========================================================================
        [HttpPost]
        public IActionResult Create([FromBody] CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                Members = _context.Users
                    .Where(u => dto.MemberIds.Contains(u.Id))
                    .ToList()
            };

            _context.Groups.Add(group);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
        }

        // ========================================================================
        // DTO do aktualizacji grupy
        // ========================================================================
        public class UpdateGroupDto
        {
            public string Name { get; set; } = "";
            public List<int> MemberIds { get; set; } = new();
        }

        // ========================================================================
        // PUT /api/groups/{id}
        // ========================================================================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateGroupDto dto)
        {
            var group = _context.Groups
                .Include(g => g.Members)
                .FirstOrDefault(g => g.Id == id);
            if (group == null) return NotFound();

            group.Name = dto.Name;
            group.Members = _context.Users
                .Where(u => dto.MemberIds.Contains(u.Id))
                .ToList();

            _context.SaveChanges();
            return NoContent();
        }

        // ========================================================================
        // DELETE /api/groups/{id}
        // ========================================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var group = _context.Groups.Find(id);
            if (group == null) return NotFound();
            _context.Groups.Remove(group);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
