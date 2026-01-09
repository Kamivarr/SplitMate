using System.Collections.Generic;

namespace SplitMate.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public List<Expense> ExpensesShared { get; set; } = new List<Expense>();
    }
}
