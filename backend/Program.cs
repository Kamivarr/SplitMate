using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;
using SplitMate.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Bezpieczne pobieranie Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                       ?? throw new InvalidOperationException("No connection string configured.");

// 2. Pobieranie tokena i konwersja na bajty (naprawiony błąd nazewnictwa)
var tokenValue = builder.Configuration.GetSection("AppSettings:Token").Value 
                 ?? "super_tajny_i_bardzo_dlugi_klucz_do_splitmate_1234567890_abcdef_ghijk_lmnop_qrstuv";

// Klucz musi mieć min. 64 znaki dla HMAC-SHA512
if (tokenValue.Length < 64) {
    tokenValue = tokenValue.PadRight(64, '!'); 
}
var jwtKeyBytes = Encoding.UTF8.GetBytes(tokenValue);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SplitMate API", Version = "v1" });

    // Dodajemy definicję zabezpieczeń JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Wpisz: Bearer {twój_token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// 3. Konfiguracja Autentykacji (używamy jwtKeyBytes)
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        // Tutaj był błąd - zmieniono 'key' na 'new SymmetricSecurityKey(jwtKeyBytes)'
        IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// 4. Inicjalizacja bazy danych (Seeding)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    int retries = 10;
    while (retries > 0)
    {
        try
        {
            Console.WriteLine($"Próba połączenia z bazą... (zostało prób: {retries})");
            db.Database.EnsureCreated(); 
            DbSeeder.Seed(db);           
            Console.WriteLine("Sukces: Połączono z bazą i zainicjowano dane!");
            break; 
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0) Console.WriteLine($"Błąd krytyczny bazy: {ex.Message}");
            else {
                Console.WriteLine($"Oczekiwanie na bazę danych... {ex.Message}");
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();