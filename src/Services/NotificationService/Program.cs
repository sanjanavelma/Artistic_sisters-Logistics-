using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Consumers;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<NotificationDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("NotificationDB")));

// ── 2. EMAIL SENDER ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailSender, EmailSender>();

// ── 3. RABBITMQ + ALL CONSUMERS ───────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Identity events
    x.AddConsumer<CustomerRegisteredConsumer>();
    x.AddConsumer<CustomerApprovedConsumer>();

    // Order events
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<DeliveryCompletedConsumer>();
    x.AddConsumer<CustomCommissionConsumer>();

    // Payment/Saga events
    x.AddConsumer<DispatchConfirmedConsumer>();

    // Logistics events
    x.AddConsumer<SLAAtRiskConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:User"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Auto configure all consumer endpoints
        cfg.ConfigureEndpoints(ctx);
    });
});

// ── 4. CONTROLLERS & OPENAPI ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── AUTO MIGRATE ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
