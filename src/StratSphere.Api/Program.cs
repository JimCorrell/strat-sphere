using Microsoft.EntityFrameworkCore;
using StratSphere.Api.Hubs;
using StratSphere.Api.Middleware;
using StratSphere.Infrastructure.Data;
using StratSphere.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "StratLeague API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<StratLeagueDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("StratLeague.Infrastructure")
    )
);

// Multi-tenancy
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<IDraftNotificationService, DraftNotificationService>();

// CORS (configure for your frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication (placeholder - implement JWT)
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Tenant resolution middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hubs
app.MapHub<DraftHub>("/hubs/draft");

app.Run();

