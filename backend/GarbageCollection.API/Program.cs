using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using GarbageCollection.Common.Settings;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using GarbageCollection.DataAccess.Repositories;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Cloudinary ────────────────────────────────────────────────────────────────
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// ── Dependency Injection ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GarbageCollection Waste Collection API",
        Version = "v1",
        Description = "API quản lý báo cáo rác thải - GarbageCollection"
    });

    // Bật XML comments từ file .xml được sinh ra khi build
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // JWT Bearer – dùng khi đã tích hợp authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Giới hạn kích thước file upload (tối đa 10 MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// ── Seed dữ liệu test ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Citizens.Any())
    {
        db.Citizens.Add(new GarbageCollection.Common.Models.Citizen
        {
            FullName = "Test Citizen",
            Email = "test@GarbageCollection.com",
            TotalPoints = 0
        });
        db.SaveChanges();
    }
}

// ── Global Exception Handler ──────────────────────────────────────────────────
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new
    {
        error = ex?.Message,
        detail = ex?.InnerException?.Message
    });
}));

// Swagger luôn bật để tiện test (giới hạn lại khi deploy production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarbageCollection Waste API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();
app.Run();
