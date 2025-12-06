using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja DbContext (pobiera ConnectionString z appsettings lub zmiennej środowiskowej)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                       ?? throw new InvalidOperationException("No connection string configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dodaj MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Pozwól na CORS z localhost (frontend w przyszłości)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

var app = builder.Build();

// Automatyczne zastosowanie migracji i seed (jeśli DB gotowa)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Czekamy chwilę na DB w środowisku Dockera (opcjonalnie można dodać retry)
    // Tutaj proste podejście: próbujemy zastosować migracje i seed; jeśli baza nie jest jeszcze dostępna, aplikacja może się nie uruchomić — docker-compose będzie restartować usługę.
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();
