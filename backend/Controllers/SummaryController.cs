using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;

namespace SplitMate.Api.Controllers
{
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
                .Where(e => e.GroupId == groupId)
                .ToList();

            var balances = new Dictionary<int, decimal>();

            foreach (var expense in expenses)
            {
                var share = expense.Amount / expense.SharedWithUsers.Count;
                foreach (var user in expense.SharedWithUsers)
                {
                    if (user.Id == expense.PaidByUserId) continue;
                    if (!balances.ContainsKey(user.Id)) balances[user.Id] = 0;
                    balances[user.Id] -= share;

                    if (!balances.ContainsKey(expense.PaidByUserId)) balances[expense.PaidByUserId] = 0;
                    balances[expense.PaidByUserId] += share;
                }
            }

            // Transformacja na listę „kto komu ile”
            var result = new List<object>();
            foreach (var kvp in balances.Where(b => b.Value > 0))
            {
                result.Add(new { toUserId = kvp.Key, amount = kvp.Value });
            }

            return Ok(result);
        }
    }
}
