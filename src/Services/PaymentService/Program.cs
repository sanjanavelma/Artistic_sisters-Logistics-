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

// ── 3. CONTROLLERS & SWAGGER ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── SWAGGER / OPENAPI ──────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "Handles Razorpay payment lifecycle: create Razorpay order, verify payment signature, query payment status by Order ID, and inspect Dispatch Saga state."
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
        .GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

// ── SWAGGER UI ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();
app.Run();
