namespace SplitMate.Api.Models
{
    public class ExpenseShare
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }
        public Expense Expense { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Kwota którą ten uczestnik powinien pokryć (część wydatku)
        public decimal Amount { get; set; }
    }
}
