using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Consumers;
using PaymentService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PaymentDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("PaymentDB")));

// ── 2. RABBITMQ + ALL CONSUMERS ───────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register all consumers — each listens to its own queue
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<ReadyForDispatchConsumer>();
    x.AddConsumer<AgentAssignedConsumer>();
    x.AddConsumer<AssignAgentFailedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:User"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Auto configure endpoints for all consumers
        cfg.ConfigureEndpoints(ctx);
    });
});

// ── 3. CONTROLLERS & OPENAPI ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── AUTO MIGRATE ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
