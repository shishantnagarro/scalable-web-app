using Microsoft.EntityFrameworkCore;
using ScalableWebApp.Data;
using ScalableWebApp.Services;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleNotificationService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AWS Services
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

// Custom Services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ILogService, LogService>();

// Database Context (MySQL by default). To switch to PostgreSQL, see README.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// Root + health endpoints for ELB/EB health checks
app.MapGet("/", () => Results.Ok(new { name = "ScalableWebApp", version = "1.0.0", utc = DateTime.UtcNow }))
   .WithName("Root");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", utc = DateTime.UtcNow }))
   .WithName("Health");

app.Run();
