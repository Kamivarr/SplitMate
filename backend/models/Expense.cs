using System.Collections.Generic;

namespace SplitMate.Api.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        
        public bool IsSettlement { get; set; } = false;

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public int PaidByUserId { get; set; }
        public User? PaidByUser { get; set; }

        public List<User> SharedWithUsers { get; set; } = new List<User>();
    }
}