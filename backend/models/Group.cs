using System.Collections.Generic;

namespace SplitMate.Api.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Many-to-many: Users <-> Groups (EF Core utworzy tabelę łączącą)
        public ICollection<User> Members { get; set; } = new List<User>();

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
