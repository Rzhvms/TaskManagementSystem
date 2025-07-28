using System.Data;
using System.Text;
using Dapper.FluentMap;
using FluentMigrator.Runner;
using HealthChecksLibrary.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using TaskService.Data.Mappings;
using TaskService.Data.Migrations;
using TaskService.Data.Repositories;
using TaskService.Data.Repositories.Interfaces;
using TaskService.Logic.Services;
using TaskService.Logic.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var jwtConfig = configuration.GetSection("Jwt");
var connectionString = configuration.GetConnectionString("Connection");
var notificationBaseUrl = configuration.GetValue<string>("NotificationService:BaseUrl");

// Database
builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));

// DI
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<INotificationClient, NotificationClient>();

// FluentMap
FluentMapper.Initialize(config => config.AddMap(new JobMap()));

// HttpClient
builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
{
    client.BaseAddress = new Uri(notificationBaseUrl);
});

// FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CreateJobsAndJobHistoriesTables).Assembly).For.Migrations())
    .AddLogging(logging => logging.AddFluentMigratorConsole());

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig["Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskService API", 
        Version = "v1"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Введите JWT токен",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, [] }
    });
});

// Health Checks
builder.Services.AddDefaultHealthChecks();

var app = builder.Build();

// Health Checks
app.MapDefaultHealthChecks();

// Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();