// ── Imports ───────────────────────────────────────────────────────────────────
using Hangfire;
using Hangfire.SqlServer;
using LogisticsService.Application.Commands.AssignAgent;
using LogisticsService.Application.Consumers;
using LogisticsService.Application.Jobs;
using LogisticsService.Infrastructure.Cache;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── 1. SQL SERVER — LOGISTICS DATABASE ───────────────────────────────────────
builder.Services.AddDbContext<LogisticsDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("LogisticsDB")));

// ── 2. REDIS CACHE ────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!));
builder.Services.AddScoped<ILogisticsCache, LogisticsCache>();

// ── 3. MEDIATR ────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<AssignAgentHandler>());

// ── 4. RABBITMQ + CONSUMERS ───────────────────────────────────────────────────
// Register both consumers so they listen to their queues automatically
builder.Services.AddMassTransit(x =>
{
    // Register the consumers — MassTransit creates queues for them
    x.AddConsumer<AssignAgentConsumer>();
    x.AddConsumer<CompensateAssignmentConsumer>();
    x.AddConsumer<DeliveryAgentRegisteredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:User"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Auto configure endpoints for all registered consumers
        cfg.ConfigureEndpoints(ctx);
    });
});

// ── 5. HANGFIRE SETUP ─────────────────────────────────────────────────────────
// Hangfire stores job data in HangfireDB SQL Server
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("HangfireDB")));

// Add Hangfire server — this processes the jobs
builder.Services.AddHangfireServer();

// Register SLA Monitor Job for dependency injection
builder.Services.AddScoped<SLAMonitorJob>();

// ── 6. CONTROLLERS & OPENAPI ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// ── BUILD APP ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── AUTO MIGRATE DATABASES ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    // Create LogisticsDB
    var db = scope.ServiceProvider
        .GetRequiredService<LogisticsDbContext>();
    db.Database.Migrate();
}

// ── MIDDLEWARE ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Hangfire Dashboard — view all jobs at /hangfire
// Shows running jobs, completed jobs, failed jobs
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();
app.MapControllers();

// ── SCHEDULE RECURRING JOB ───────────────────────────────────────────────────
// "*/5 * * * *" = every 5 minutes (cron expression)
// Job ID "sla-monitor" = unique name so it doesn't duplicate
RecurringJob.AddOrUpdate<SLAMonitorJob>(
    "sla-monitor",
    job => job.CheckSLADeadlines(),
    "*/5 * * * *");

app.Run();
