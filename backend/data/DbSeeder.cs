using SplitMate.Api.Data;
using SplitMate.Api.Models;
using System.Security.Cryptography; // Potrzebne do haseł
using System.Text;

namespace SplitMate.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Users.Any()) return;

            AddUser(context, "Kamil", "kamil123");
            AddUser(context, "Anna", "anna123");
            context.SaveChanges();
        }

        private static void AddUser(AppDbContext context, string name, string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            context.Users.Add(new User
            {
                Name = name,
                Login = name.ToLower(),
                PasswordHash = hash,
                PasswordSalt = salt
            });
        }
    }
}