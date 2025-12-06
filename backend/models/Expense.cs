using System.Collections.Generic;

namespace SplitMate.Api.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        // Która grupa
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        // Kto zapłacił
        public int PaidByUserId { get; set; }
        public User? PaidByUser { get; set; }

        // Wiele-do-wielu: użytkownicy, którzy dzielą koszt
        public List<User> SharedWithUsers { get; set; } = new List<User>();
    }
}
