using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SplitMate.Api.Controllers
{
    /// <summary>
    /// Kontroler odpowiedzialny za obliczanie sald grupowych oraz proces rozliczeń między użytkownikami.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Rozliczenia i Podsumowania")]
    public class SummaryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SummaryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pobiera podsumowanie długów dla konkretnej grupy.
        /// Oblicza, kto jest komu winien pieniądze na podstawie wydatków i zarejestrowanych spłat.
        /// </summary>
        /// <param name="groupId">Identyfikator grupy.</param>
        /// <returns>Lista obiektów DebtDto reprezentujących aktualne zadłużenia.</returns>
        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<DebtDto>>> GetGroupSummary(int groupId)
        {
            // Pobieramy wszystkie wydatki grupy wraz z uczestnikami i płatnikiem
            var expenses = await _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.GroupId == groupId)
                .ToListAsync();

            // Słownik przechowujący bilanse: (ID_dłużnika, ID_odbiorcy) -> łączna kwota
            var balances = new Dictionary<(int from, int to), decimal>();

            foreach (var exp in expenses)
            {
                if (exp.IsSettlement)
                {
                    // Obsługa spłaty: PaidByUserId oddaje pieniądze pierwszej osobie z SharedWithUsers
                    var receiver = exp.SharedWithUsers.FirstOrDefault();
                    if (receiver != null)
                    {
                        var key = (from: exp.PaidByUserId, to: receiver.Id);
                        balances[key] = balances.GetValueOrDefault(key) - exp.Amount;
                    }
                }
                else if (exp.SharedWithUsers.Any())
                {
                    // Obsługa zwykłego wydatku: dzielimy kwotę równo na wszystkich uczestników
                    var share = exp.Amount / exp.SharedWithUsers.Count;
                    foreach (var user in exp.SharedWithUsers.Where(u => u.Id != exp.PaidByUserId))
                    {
                        var key = (from: user.Id, to: exp.PaidByUserId);
                        balances[key] = balances.GetValueOrDefault(key) + share;
                    }
                }
            }

            // Pobieramy nazwy użytkowników biorących udział w rozliczeniach, aby uniknąć N+1 zapytań w pętli
            var involvedUserIds = balances.Keys.SelectMany(k => new[] { k.from, k.to }).Distinct();
            var userNames = await _context.Users
                .Where(u => involvedUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            // Budujemy wynik końcowy, filtrując znikome kwoty i zaokrąglając do 2 miejsc po przecinku
            var result = balances
                .Where(b => b.Value > 0.01m)
                .Select(b => new DebtDto 
                {
                    FromUserId = b.Key.from,
                    FromUserName = userNames.GetValueOrDefault(b.Key.from) ?? "Nieznany",
                    ToUserId = b.Key.to,
                    ToUserName = userNames.GetValueOrDefault(b.Key.to) ?? "Nieznany",
                    Amount = Math.Round(b.Value, 2)
                })
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Rejestruje spłatę długu między dwoma użytkownikami.
        /// Tworzy specjalny wpis w tabeli Expenses z flagą IsSettlement.
        /// </summary>
        /// <param name="dto">Dane dotyczące spłaty.</param>
        [HttpPost("settle")]
    public async Task<IActionResult> SettleDebt([FromBody] SettleDebtDto dto)
    {
        // 1. Pobieramy ID zalogowanego użytkownika z Tokena JWT
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (currentUserIdClaim == null) return Unauthorized();
        
        int currentUserId = int.Parse(currentUserIdClaim.Value);

        // 2. Logika bezpieczeństwa: Tylko odbiorca długu (Kamil) może potwierdzić, że go dostał
        if (currentUserId != dto.ToUserId)
        {
            return Forbid("Tylko osoba otrzymująca pieniądze może potwierdzić spłatę.");
        }

        var receiver = await _context.Users.FindAsync(dto.ToUserId);
        if (receiver == null) return NotFound("Odbiorca płatności nie istnieje.");

        var settlement = new Expense
        {
            Description = $"Rozliczenie: {dto.FromUserName} -> {dto.ToUserName}",
            Amount = dto.Amount,
            GroupId = dto.GroupId,
            PaidByUserId = dto.FromUserId, // Dłużnik
            IsSettlement = true,
            SharedWithUsers = new List<User> { receiver }
        };

        _context.Expenses.Add(settlement);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Spłata została potwierdzona przez odbiorcę." });
    }

        #region DTOs

        /// <summary>
        /// Obiekt transferu danych dla informacji o zadłużeniu.
        /// </summary>
        public class DebtDto 
        {
            public int FromUserId { get; set; }
            public string FromUserName { get; set; } = string.Empty;
            public int ToUserId { get; set; }
            public string ToUserName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        /// <summary>
        /// Obiekt transferu danych dla procesu spłaty długu.
        /// </summary>
        public class SettleDebtDto
        {
            public int GroupId { get; set; }
            public int FromUserId { get; set; }
            public string FromUserName { get; set; } = string.Empty;
            public int ToUserId { get; set; }
            public string ToUserName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        #endregion
    }
}