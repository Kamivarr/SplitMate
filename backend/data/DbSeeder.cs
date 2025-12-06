using SplitMate.Api.Data;
using SplitMate.Api.Models;

namespace SplitMate.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext ctx)
        {
            if (!ctx.Users.Any())
            {
                var user1 = new User { Name = "Kamil" };
                var user2 = new User { Name = "Ania" };
                var user3 = new User { Name = "Marek" };

                ctx.Users.AddRange(user1, user2, user3);
                ctx.SaveChanges();
            }

            if (!ctx.Groups.Any())
            {
                var group1 = new Group { Name = "Wakacje 2025", Members = ctx.Users.Take(3).ToList() };
                var group2 = new Group { Name = "Kolacja w restauracji", Members = ctx.Users.Take(2).ToList() };

                ctx.Groups.AddRange(group1, group2);
                ctx.SaveChanges();
            }

            if (!ctx.Expenses.Any())
            {
                var userList = ctx.Users.ToList();
                var groupList = ctx.Groups.ToList();

                var expense1 = new Expense
                {
                    Description = "Pizza",
                    Amount = 60,
                    GroupId = groupList[1].Id,
                    PaidByUserId = userList[0].Id,
                    SharedWithUsers = userList.Take(2).ToList()
                };

                var expense2 = new Expense
                {
                    Description = "Bilety do muzeum",
                    Amount = 90,
                    GroupId = groupList[0].Id,
                    PaidByUserId = userList[1].Id,
                    SharedWithUsers = userList.Take(3).ToList()
                };

                ctx.Expenses.AddRange(expense1, expense2);
                ctx.SaveChanges();
            }
        }
    }
}
