using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Models;

namespace SplitMate.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseShare> ExpenseShares => Set<ExpenseShare>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Wiele-do-wielu: Expense <-> User (SharedWithUsers)
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.SharedWithUsers)
                .WithMany(u => u.ExpensesShared)
                .UsingEntity(j => j.ToTable("ExpenseSharedUsers"));

            // Jeden-do-wielu: Expense.PaidByUser
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidByUser)
                .WithMany()
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Jeden-do-wielu: Expense -> Group
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Group)
                .WithMany(g => g.Expenses)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
