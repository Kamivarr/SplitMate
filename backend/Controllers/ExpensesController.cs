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

        // ========================================================================
        // DTO dla wydatku
        // ========================================================================
        public class ExpenseDto
        {
            public int Id { get; set; }
            public string Description { get; set; } = "";
            public decimal Amount { get; set; }
            public int GroupId { get; set; }
            public int PaidByUserId { get; set; }
            public string PaidByUserName { get; set; } = "";
            public List<UserDto> SharedWithUsers { get; set; } = new();
        }

        public class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        // ========================================================================
        // GET /api/expenses
        // ========================================================================
        [HttpGet]
        public IActionResult GetAll()
        {
            var expenses = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    GroupId = e.GroupId,
                    PaidByUserId = e.PaidByUserId,
                    PaidByUserName = e.PaidByUser.Name,
                    SharedWithUsers = e.SharedWithUsers
                        .Select(u => new UserDto { Id = u.Id, Name = u.Name })
                        .ToList()
                })
                .ToList();

            return Ok(expenses);
        }

        // ========================================================================
        // GET /api/expenses/{id}
        // ========================================================================
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .Where(e => e.Id == id)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Description = e.Description,
                    Amount = e.Amount,
                    GroupId = e.GroupId,
                    PaidByUserId = e.PaidByUserId,
                    PaidByUserName = e.PaidByUser.Name,
                    SharedWithUsers = e.SharedWithUsers
                        .Select(u => new UserDto { Id = u.Id, Name = u.Name })
                        .ToList()
                })
                .FirstOrDefault();

            if (expense == null)
                return NotFound();

            return Ok(expense);
        }

        // ========================================================================
        // POST /api/expenses/from-dto
        // ========================================================================
        public class CreateExpenseDto
        {
            public string Description { get; set; } = "";
            public decimal Amount { get; set; }
            public int GroupId { get; set; }
            public int PaidByUserId { get; set; }
            public List<int> SharedWithUserIds { get; set; } = new();
        }

        [HttpPost("from-dto")]
        public IActionResult CreateFromDto([FromBody] CreateExpenseDto dto)
        {
            var expense = new Expense
            {
                Description = dto.Description,
                Amount = dto.Amount,
                GroupId = dto.GroupId,
                PaidByUserId = dto.PaidByUserId,
                SharedWithUsers = _context.Users
                    .Where(u => dto.SharedWithUserIds.Contains(u.Id))
                    .ToList()
            };

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            var expenseDto = new ExpenseDto
            {
                Id = expense.Id,
                Description = expense.Description,
                Amount = expense.Amount,
                GroupId = expense.GroupId,
                PaidByUserId = expense.PaidByUserId,
                PaidByUserName = _context.Users
                    .Where(u => u.Id == expense.PaidByUserId)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "",
                SharedWithUsers = expense.SharedWithUsers
                    .Select(u => new UserDto { Id = u.Id, Name = u.Name })
                    .ToList()
            };

            return CreatedAtAction(nameof(Get), new { id = expense.Id }, expenseDto);
        }

        // ========================================================================
        // DELETE /api/expenses/{id}
        // ========================================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var expense = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .FirstOrDefault(e => e.Id == id);

            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
