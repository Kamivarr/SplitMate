using SplitMate.Api.Data;
using SplitMate.Api.Models;
using Microsoft.EntityFrameworkCore; // Dodane do obsługi ExecuteSqlRawAsync
using System.Security.Cryptography;
using System.Text;

namespace SplitMate.Api.Data
{
    public static class DbSeeder
    {
        // Zmieniono na async Task, aby obsłużyć asynchroniczne zapytanie SQL
        public static async Task Seed(AppDbContext context)
        {
            // ---------------------------------------------------------
            // 1. TWORZENIE PROCEDURY SKŁADOWANEJ (STORED PROCEDURE)
            // ---------------------------------------------------------
            // Tworzymy procedurę, która usuwa wszystkie wydatki dla danej grupy.
            // "OR REPLACE" sprawia, że jeśli procedura istnieje, zostanie zaktualizowana.
            var createProcSql = @"
                CREATE OR REPLACE PROCEDURE ResetGroupExpenses(p_groupId INT)
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    -- Usuwamy wszystkie wydatki danej grupy.
                    -- Powiązane rekordy w tabelach łączących usuną się automatycznie
                    -- dzięki kaskadowaniu kluczy obcych (Cascade Delete).
                    DELETE FROM ""Expenses"" WHERE ""GroupId"" = p_groupId;
                END;
                $$;";

            await context.Database.ExecuteSqlRawAsync(createProcSql);

            // ---------------------------------------------------------
            // 2. SEEDOWANIE DANYCH (UŻYTKOWNICY I WYDATKI)
            // ---------------------------------------------------------
            if (context.Users.Any()) return;

            AddUser(context, "Kamil", "kamil123");
            AddUser(context, "Anna", "anna123");
            AddUser(context, "Arek", "kamil123"); // Dodatkowy użytkownik z readme

            await context.SaveChangesAsync();

            if (!context.Expenses.Any())
            {
                // Upewniamy się, że użytkownik istnieje przed przypisaniem
                var kamil = await context.Users.FirstOrDefaultAsync(u => u.Login == "kamil");
                if (kamil != null)
                {
                    // Ponieważ nie mamy jeszcze grup w seedzie, tworzymy przykładową,
                    // aby wydatek miał poprawne GroupId (wymagane przez FK).
                    var group = new Group
                    {
                        Name = "Wyjazd w Tatry",
                        Members = new List<User> { kamil }
                    };
                    context.Groups.Add(group);
                    await context.SaveChangesAsync(); // Zapisz grupę, by dostała ID

                    context.Expenses.Add(new Expense
                    {
                        Description = "Pizza na integrację",
                        Amount = 120.50m,
                        PaidByUserId = kamil.Id,
                        GroupId = group.Id, // Przypisanie do grupy
                        IsSettlement = false
                    });
                    await context.SaveChangesAsync();
                }
            }
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