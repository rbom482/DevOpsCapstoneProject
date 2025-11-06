using LogiTrack;
using LogiTrack.Models;
using LogiTrack.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogiTrack API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Add custom error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Add test inventory items if none exist
    if (!context.InventoryItems.Any())
    {
        var items = new List<InventoryItem>
        {
            new InventoryItem { Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" },
            new InventoryItem { Name = "Forklift", Quantity = 3, Location = "Warehouse B" },
            new InventoryItem { Name = "Hand Truck", Quantity = 25, Location = "Warehouse A" }
        };

        context.InventoryItems.AddRange(items);
        context.SaveChanges();
        
        Console.WriteLine("Database seeded with initial inventory items.");
    }
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
