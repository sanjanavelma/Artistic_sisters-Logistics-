using OrderService.Middleware;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Consumers;
using OrderService.Infrastructure.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OrderDB")));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddMassTransit(x => {
    // Receives agent status updates published by LogisticsService
    x.AddConsumer<DeliveryStatusSyncConsumer>();

    x.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h => {
            h.Username(builder.Configuration["RabbitMQ:User"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
        cfg.ConfigureEndpoints(ctx);
    });
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
builder.Services.AddScoped<OrderService.Infrastructure.Cache.OrderCache>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── SWAGGER / OPENAPI ──────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Manages customer orders and custom commissions: place order, place commission, update status, and retrieve orders by customer or ID."
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}
// ── SWAGGER UI ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service v1");
    c.RoutePrefix = "swagger";
});
// Catch DomainException → 400, anything else → 500, always as JSON
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
