using SplitMate.Api.Data;
using SplitMate.Api.Models;

public static class DbSeeder
{
    public static void Seed(AppDbContext ctx)
    {
        if (!ctx.Users.Any())
        {
            var users = new List<User>
            {
                new User { Name = "Kamil" },
                new User { Name = "Anna" },
                new User { Name = "Tomek" }
            };
            ctx.Users.AddRange(users);
            ctx.SaveChanges();
        }

        if (!ctx.Groups.Any())
        {
            var group1 = new Group { Name = "Wspólne wydatki", Members = ctx.Users.Take(2).ToList() };
            var group2 = new Group { Name = "Domowe", Members = ctx.Users.Skip(1).ToList() };
            ctx.Groups.AddRange(group1, group2);
            ctx.SaveChanges();
        }
    }
}
