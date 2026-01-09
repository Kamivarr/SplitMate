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
    public class SummaryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SummaryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("group/{groupId}")]
        public IActionResult GetGroupSummary(int groupId)
        {
            var expenses = _context.Expenses
                .Include(e => e.SharedWithUsers)
                .Include(e => e.PaidByUser)
                .Where(e => e.GroupId == groupId)
                .ToList();

            // słownik bilansów: (odUserId -> doUserId) = kwota
            var balances = new Dictionary<(int fromUserId, int toUserId), decimal>();

            foreach (var expense in expenses)
            {
                if (expense.SharedWithUsers.Count == 0) continue;

                var share = expense.Amount / expense.SharedWithUsers.Count;

                foreach (var user in expense.SharedWithUsers)
                {
                    if (user.Id == expense.PaidByUserId) continue;

                    var key = (fromUserId: user.Id, toUserId: expense.PaidByUserId);

                    // sumujemy wszystkie wydatki z tej pary
                    if (!balances.ContainsKey(key))
                        balances[key] = 0;
                    balances[key] += share;
                }
            }

            // transformacja na listę z nazwami użytkowników
            var result = balances.Select(b =>
            {
                var fromName = _context.Users.FirstOrDefault(u => u.Id == b.Key.fromUserId)?.Name ?? "Nieznany";
                var toName = _context.Users.FirstOrDefault(u => u.Id == b.Key.toUserId)?.Name ?? "Nieznany";
                return new
                {
                    fromUserName = fromName,
                    toUserName = toName,
                    amount = Math.Round(b.Value, 2)
                };
            }).ToList();

            return Ok(result);
        }

    }
}
