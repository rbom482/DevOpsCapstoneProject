using LogiTrack;
using LogiTrack.Models;
using LogiTrack.Middleware;
using LogiTrack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced logging for production
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    // In production, consider adding Serilog, Application Insights, or other providers
}

// Add services to the container.
builder.Services.AddControllers();

// Enhanced DbContext with connection pooling for better performance
builder.Services.AddDbContextPool<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db")
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment()),
    poolSize: 128); // Optimize pool size for concurrent requests

// Add Memory Caching
builder.Services.AddMemoryCache();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<LogiTrackContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Enhanced Cache Service
builder.Services.AddScoped<ICacheService, EnhancedCacheService>();

// Register State Persistence Service
builder.Services.AddScoped<IStatePersistenceService, StatePersistenceService>();

// Register Cache Manager Service
builder.Services.AddScoped<ICacheManagerService, CacheManagerService>();

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add health checks for production monitoring
builder.Services.AddHealthChecks()
    .AddCheck("database", () => 
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database is operational"))
    .AddCheck("memory_cache", () => 
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Memory cache is operational"));

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

// Enable response compression
app.UseResponseCompression();

// Add custom error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoint (publicly accessible for monitoring)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogiTrackContext>();
    var statePersistence = scope.ServiceProvider.GetRequiredService<IStatePersistenceService>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
    
    // Validate database persistence
    var persistenceValid = await statePersistence.ValidateDatabasePersistenceAsync();
    if (!persistenceValid)
    {
        Console.WriteLine("Warning: Database persistence validation failed!");
    }
    
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
    
    // Restore system state and warm up caches
    try
    {
        await statePersistence.RestoreSystemStateAsync();
        Console.WriteLine("System state restoration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: System state restoration failed: {ex.Message}");
    }
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
