using LogiTrack;
using LogiTrack.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Test the database and models
using (var context = new LogiTrackContext())
{
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
    }

    // Retrieve and print inventory to confirm
    var allItems = context.InventoryItems.ToList();
    Console.WriteLine("Current Inventory:");
    foreach (var item in allItems)
    {
        item.DisplayInfo();
    }

    // Test Order functionality with proper EF Core relationships
    var order = new Order
    {
        CustomerName = "Samir",
        DatePlaced = new DateTime(2025, 4, 5)
    };

    // Add the order to context first to get an OrderId
    context.Orders.Add(order);
    context.SaveChanges();

    // Add items to the order using the improved AddItem method
    var palletJack = allItems.FirstOrDefault(i => i.Name == "Pallet Jack");
    var forklift = allItems.FirstOrDefault(i => i.Name == "Forklift");

    if (palletJack != null)
        order.AddItem(palletJack, 2); // Order 2 pallet jacks

    if (forklift != null)
        order.AddItem(forklift, 1); // Order 1 forklift

    // Save the order items
    context.SaveChanges();

    // Print enhanced order summary
    Console.WriteLine("\nOrder Summary:");
    order.PrintOrderDetails();
    Console.WriteLine($"Total Quantity: {order.GetTotalQuantity()}");

    // Test removing an item
    if (forklift != null)
    {
        order.RemoveItem(forklift.ItemId);
        context.SaveChanges();
        
        Console.WriteLine("\nAfter removing forklift:");
        order.PrintOrderDetails();
        Console.WriteLine($"Total Quantity: {order.GetTotalQuantity()}");
    }
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
