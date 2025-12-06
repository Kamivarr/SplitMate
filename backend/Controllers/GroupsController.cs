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

        [HttpGet]
        public IActionResult GetAll()
        {
            var groups = _context.Groups
                .Include(g => g.Members)
                .ToList();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var group = _context.Groups
                .Include(g => g.Members)
                .FirstOrDefault(g => g.Id == id);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Group group)
        {
            _context.Groups.Add(group);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = group.Id }, group);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Group updatedGroup)
        {
            var group = _context.Groups
                .Include(g => g.Members)
                .FirstOrDefault(g => g.Id == id);
            if (group == null) return NotFound();

            group.Name = updatedGroup.Name;
            group.Members = updatedGroup.Members;

            _context.SaveChanges();
            return NoContent();
        }

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
