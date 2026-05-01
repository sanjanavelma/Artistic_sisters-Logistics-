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
    x.AddConsumer<DeliveryAgentRegisteredConsumer>();

    // Order events
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<DeliveryCompletedConsumer>();
    x.AddConsumer<CustomCommissionConsumer>();

    // Payment/Saga events
    x.AddConsumer<DispatchConfirmedConsumer>();

    // Logistics events
    x.AddConsumer<AgentAssignedConsumer>();
    x.AddConsumer<SLAAtRiskConsumer>();

    // Status sync — emails Customer + Agent + Admin on every status change
    x.AddConsumer<OrderStatusChangedConsumer>();

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

// ── 4. CONTROLLERS & SWAGGER ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── SWAGGER / OPENAPI ──────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Provides visibility into the notification system: browse the last 50 notification logs and view active email templates."
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

var app = builder.Build();

// ── AUTO MIGRATE ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<NotificationDbContext>();
    db.Database.Migrate();
}

// ── SWAGGER UI ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();
app.Run();
