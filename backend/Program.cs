using Microsoft.EntityFrameworkCore;
using DotNetEnv;

Env.Load();
string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

WebApplicationOptions options = new WebApplicationOptions
{
    EnvironmentName = environment
};
var builder = WebApplication.CreateBuilder(options);

builder.Configuration.AddEnvironmentVariables();

var dbConnString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(dbConnString));

var redisConnString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = redisConnString;
});

builder.Services.AddControllers(opt => opt.Conventions.Insert(0, new GlobalRoutePrefixConvention("api/v1")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services
    .AddConfig(builder.Configuration)
    .AddMyDependencyGroup()
    .AddCustomCors(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database connection established and migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while connecting to the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();
app.Run();
