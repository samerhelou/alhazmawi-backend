using System.Text;
using AlHazmawi.Api.Hubs;
using AlHazmawi.Application;
using AlHazmawi.Infrastructure;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add Layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Controllers & SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<AlHazmawi.Application.Common.Interfaces.IOrderNotificationService, AlHazmawi.Api.Services.OrderNotificationService>();

// Configure CORS for Mobile and Web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Secret"] 
    ?? "AlHazmawiDeliverySuperSecretKey2026!#VerySecureKeyWithMinimum32BytesLength";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "AlHazmawiDelivery";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "AlHazmawiDeliveryUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Allow SignalR hubs to extract JWT token from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/orders") || path.StartsWithSegments("/hubs/driver-location")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "الحزماوي للتوصيل - Al-Hazmawi Delivery API",
        Version = "v1",
        Description = "نظام التوصيل المتكامل: المطاعم، المتاجر، الزبائن، المناديب، ولوحة التحكم."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Al-Hazmawi Delivery API v1");
    c.RoutePrefix = string.Empty; // Swagger as home page for easy testing
});

app.UseStaticFiles();

// Create uploads directory if not exists
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");
app.MapHub<DriverLocationHub>("/hubs/driver-location");

// Auto-migrate database and seed initial data (SqlServer: Migrate, PostgreSQL: EnsureCreated for free tier)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<AlHazmawiDbContext>();
        var connStr = app.Configuration.GetConnectionString("DefaultConnection") ?? "";
        var isPostgres = connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
        if (isPostgres)
        {
            logger.LogInformation("PostgreSQL detected - ensuring database created...");
            await dbContext.Database.EnsureCreatedAsync();
        }
        else
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
        }
        
        logger.LogInformation("Seeding database initial roles and administrator...");
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
        
        logger.LogInformation("Database ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

app.Run();
