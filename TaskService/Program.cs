using System.Data;
using Dapper.FluentMap;
using FluentMigrator.Runner;
using Npgsql;
using TaskService.Data.Interfaces;
using TaskService.Data.Mappings;
using TaskService.Data.Migrations;
using TaskService.Data.Repositories;
using TaskService.Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Подключение к базе данных
builder.Services.AddScoped<IDbConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("Connection")));

// Регистрация зависимостей
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();

// Настройка Dapper FluentMap
FluentMapper.Initialize(config =>
{
    config.AddMap(new JobMap());
});

// Настройка FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Connection"))
        .ScanIn(typeof(CreateJobsAndJobHistoriesTables).Assembly).For.Migrations())
    .AddLogging(logging => logging.AddFluentMigratorConsole());

// Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Запуск миграций
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Маршрутизация
app.MapControllers();

app.Run();