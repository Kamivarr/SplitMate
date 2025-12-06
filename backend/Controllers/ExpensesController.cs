using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;

namespace SplitMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(_context.Expenses.Include(e => e.SharedWithUsers).ToList());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);
            if (expense == null) return NotFound();
            return Ok(expense);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Expense expense)
        {
            _context.Expenses.Add(expense);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = expense.Id }, expense);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Expense updatedExpense)
        {
            var expense = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);
            if (expense == null) return NotFound();

            expense.Description = updatedExpense.Description;
            expense.Amount = updatedExpense.Amount;
            expense.GroupId = updatedExpense.GroupId;
            expense.PaidByUserId = updatedExpense.PaidByUserId;
            expense.SharedWithUsers = updatedExpense.SharedWithUsers;

            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense == null) return NotFound();
            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
