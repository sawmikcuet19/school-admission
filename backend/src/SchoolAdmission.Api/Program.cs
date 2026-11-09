using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Api.Services;
using SchoolAdmission.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is missing. Set ConnectionStrings__DefaultConnection " +
        "(or ConnectionStrings:DefaultConnection in appsettings.json).");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton(TimeProvider.System);

var holdDurationSeconds = HoldOptions.DefaultHoldDurationSeconds;

if (int.TryParse(builder.Configuration["HOLD_DURATION_SECONDS"], out var configuredDuration))
{
    holdDurationSeconds = configuredDuration;
}

builder.Services.Configure<HoldOptions>(options =>
{
    options.HoldDurationSeconds = Math.Max(0, holdDurationSeconds);
});

builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IHoldService, HoldService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddHostedService<HoldExpiryBackgroundService>();

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Admission API",
        Version = "v1",
        Description = "First-come-first-served school admission with expiring seat holds."
    });
});

var app = builder.Build();

await ApplyMigrationsAsync(app.Services);
await SeedAdminCredentialAsync(app.Services, app.Configuration);

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("frontend");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "School Admission API v1");
});
app.MapControllers();

app.Run();

static async Task ApplyMigrationsAsync(IServiceProvider services)
{
    const int maxAttempts = 20;

    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseStartup");

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied.");
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                "Database not ready (attempt {Attempt}/{MaxAttempts}): {Message}",
                attempt,
                maxAttempts,
                exception.Message);

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    await db.Database.MigrateAsync();
}

static async Task SeedAdminCredentialAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("AdminSeed");

    if (await db.AdminCredentials.AnyAsync())
    {
        logger.LogInformation("Admin verification password already initialized.");
        return;
    }

    var initialPassword = configuration["ADMIN_PASSWORD"];

    if (string.IsNullOrWhiteSpace(initialPassword))
    {
        initialPassword = "ChangeMe-Admin1";
    }

    db.AdminCredentials.Add(new AdminCredential
    {
        Id = AdminCredential.SingletonId,
        PasswordHash = AdminPasswordHasher.Hash(initialPassword),
        UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
    });

    try
    {
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded the admin verification password from ADMIN_PASSWORD.");
    }
    catch (DbUpdateException)
    {
        logger.LogInformation("Admin verification password was seeded by another instance.");
    }
}
