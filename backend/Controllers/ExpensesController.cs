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

        // ====================================================================
        // ZMODYFIKOWANA METODA CREATE Z TRANSAKCJĄ (ACID)
        // ====================================================================
        [HttpPost]
        public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseDto dto)
        {
            // 1. Rozpoczynamy jawną transakcję bazodanową.
            // Dzięki temu mamy pewność, że albo wszystko się zapisze, albo nic.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var expense = new Expense
                {
                    Description = dto.Description,
                    Amount = dto.Amount,
                    GroupId = dto.GroupId,
                    PaidByUserId = dto.PaidByUserId,
                    // Pobieramy użytkowników z bazy, aby EF Core utworzył poprawne relacje w tabeli łączącej
                    SharedWithUsers = await _context.Users
                        .Where(u => dto.SharedWithUserIds.Contains(u.Id))
                        .ToListAsync()
                };

                // 2. Dodajemy wydatek do kontekstu (w pamięci RAM)
                _context.Expenses.Add(expense);
                
                // 3. Wykonujemy INSERT do bazy danych
                await _context.SaveChangesAsync();

                // 4. Zatwierdzamy transakcję (COMMIT) - dopiero teraz dane są trwałe w bazie.
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetAll), MapToDto(expense));
            }
            catch (Exception)
            {
                // 5. W razie jakiegokolwiek błędu wycofujemy zmiany (ROLLBACK).
                // To zapobiega sytuacji, gdzie wydatek powstał, ale nie ma przypisanych ludzi.
                await transaction.RollbackAsync();
                throw; // Rzucamy błąd dalej, by API zwróciło 500
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

        // Helper do mapowania encji na DTO
        private static ExpenseDto MapToDto(Expense e) => new()
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            GroupId = e.GroupId,
            PaidByUserId = e.PaidByUserId,
            PaidByUserName = e.PaidByUser?.Name ?? "Nieznany",
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
            public List<UserDto> SharedWithUsers { get; set; } = new();
        }

        public class UserDto {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
    }
}