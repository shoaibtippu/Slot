using System.Text;
using FastEndpoints;
using Slot.Host.Hubs;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Slot.Adapters.Azure.Blob;
using Slot.Adapters.FastEndpoint;
using Slot.Adapters.PostgreSql;
using Slot.Adapters.PostgreSql.Contexts;
using Slot.Adapters.PostgreSql.DataSeeds;
using Slot.Application.Configurations;
using Slot.Common.Traits;
using Slot.Host;

var builder = WebApplication.CreateBuilder(args);

// ── Session / HTTP context ────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISessionInfoProvider, SessionInfoProvider>();

// ── JWT settings ──────────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// ── Identity ──────────────────────────────────────────────────────────
builder.Services.AddIdentityCore<IdentityUser>(opts =>
    {
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase = false;
        opts.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ── Database ──────────────────────────────────────────────────────────
builder.Services.AddPostgreSqlAdapter(builder.Configuration);

// ── Blob storage ───────────────────────────────────────────────────────
builder.Services.AddAzureBlobAdapter(builder.Configuration);

// ── JWT Authentication ────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub/chat"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── FastEndpoints + Application services ─────────────────────────────
builder.Services.AddFastEndpointAdapter();

// ── CORS ──────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ── SignalR ───────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Swagger (FastEndpoints.Swagger / NSwag) ───────────────────────────
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Slot API";
            s.Version = "v1";
        };
        o.AutoTagPathSegmentIndex = 2;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection(); 
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
    {
        c.Endpoints.ShortNames = false;
    })
    .UseSwaggerGen();

app.MapHub<ChatHub>("/hub/chat");

await AdminSeeder.SeedAsync(app.Services);
await UserSeeder.SeedAsync(app.Services);

app.Run();