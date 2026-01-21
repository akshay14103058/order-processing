using Microsoft.EntityFrameworkCore;
using OrderProcessingSystem.Application.Interfaces;
using OrderProcessingSystem.Application.Services;
using OrderProcessingSystem.Domain.Interfaces;
using OrderProcessingSystem.Infrastructure.BackgroundServices;
using OrderProcessingSystem.Infrastructure.Data;
using OrderProcessingSystem.Infrastructure.Repositories;

using Serilog;
using FluentValidation;

using OrderProcessingSystem.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// 0. Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

// 1. Data Layer
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb")); // Using InMemory for the assignment

// 2. Application Layer
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 3. Background Services
builder.Services.AddHostedService<OrderStatusUpdaterService>();

builder.Services.AddControllers();

// Auto-validation removed in favor of manual validation due to deprecation of FluentValidation.AspNetCore
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

try 
{
    var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<OrderProcessingSystem.Api.Middleware.ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
