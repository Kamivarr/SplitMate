using Microsoft.EntityFrameworkCore;
using SplitMate.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                       ?? throw new InvalidOperationException("No connection string configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

// Automatyczne migracje + seed danych
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        DbSeeder.Seed(db);
    }
    catch
    {
        // jeśli DB jeszcze nie gotowa, ignorujemy — kontener wystartuje i spróbuje ponownie
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Nasłuch na 0.0.0.0, żeby był dostępny z innych kontenerów i hosta
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5000");

app.UseCors();
app.MapControllers();

app.Run();
