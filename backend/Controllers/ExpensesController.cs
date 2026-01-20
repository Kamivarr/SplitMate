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
    [Tags("Zarządzanie Wydatkami")]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAll()
        {
            return await _context.Expenses
                .Include(e => e.PaidByUser)
                .Include(e => e.SharedWithUsers)
                .Select(e => MapToDto(e))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Pobieramy płatnika "do ręki" przed utworzeniem wydatku
                var payer = await _context.Users.FindAsync(dto.PaidByUserId);
                if (payer == null) return BadRequest("Płatnik nie istnieje.");

                var expense = new Expense
                {
                    Description = dto.Description,
                    Amount = dto.Amount,
                    GroupId = dto.GroupId,
                    PaidByUserId = dto.PaidByUserId,
                    PaidByUser = payer, // Przypisujemy obiekt, żeby EF znał imię
                    SharedWithUsers = await _context.Users
                        .Where(u => dto.SharedWithUserIds.Contains(u.Id))
                        .ToListAsync()
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetAll), MapToDto(expense));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- MAPOWANIE (TŁUMACZENIE) ---
        private static ExpenseDto MapToDto(Expense e) => new()
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            GroupId = e.GroupId,
            PaidByUserId = e.PaidByUserId,
            // Tutaj wyciągamy imię z obiektu bazy danych do stringa dla Frontendu
            PaidByUserName = e.PaidByUser?.Name ?? "Nieznany", 
            IsSettlement = e.IsSettlement,
            SharedWithUsers = e.SharedWithUsers.Select(u => new UserDto { Id = u.Id, Name = u.Name }).ToList()
        };

        public class CreateExpenseDto {
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public int GroupId { get; set; }
            public int PaidByUserId { get; set; }
            public List<int> SharedWithUserIds { get; set; } = new();
        }

        public class ExpenseDto {
            public int Id { get; set; }
            public string Description { get; set; } = "";
            public decimal Amount { get; set; }
            public int GroupId { get; set; }
            public int PaidByUserId { get; set; }
            public string PaidByUserName { get; set; } = ""; 
            public bool IsSettlement { get; set; }
            public List<UserDto> SharedWithUsers { get; set; } = new();
        }

        public class UserDto {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}