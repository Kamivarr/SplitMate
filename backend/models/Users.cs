using System.Collections.Generic;

namespace SplitMate.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Wiele-do-wielu: jakie wydatki dzieli
        public List<Expense> ExpensesShared { get; set; } = new List<Expense>();
    }
}
