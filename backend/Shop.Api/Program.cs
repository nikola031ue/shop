using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;
using Shop.Application;
using Shop.Application.Auth.Interfaces;
using Shop.Infrastructure;
using Shop.Infrastructure.Observability;
using Shop.Infrastructure.Persistence;
using Shop.Infrastructure.Settings;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ─────────────────────────────────────────────────────────────────
var otlpEndpointForLogs = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("service.name", "shop-api")
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.OpenTelemetry(o =>
    {
        o.Endpoint = otlpEndpointForLogs;
        o.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
        o.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = "shop-api",
            ["service.version"] = "1.0.0"
        };
    }));

// ── OpenTelemetry ────────────────────────────────────────────────────────────
var otlpEndpoint = otlpEndpointForLogs;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shop-api", serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o => o.RecordException = true)
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter(ShopMetrics.MeterName)
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

// ── Application services ─────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((jwtOptions, settings) =>
    {
        var s = settings.Value;
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = s.Issuer,
            ValidateAudience = true, ValidAudience = s.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SecretKey)),
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
app.UseSerilogRequestLogging(o =>
{
    o.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db, builder.Configuration, passwordHasher);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
