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
using System.Text.Json.Serialization;
using DotNetEnv;

// ── 1. Nạp biến môi trường từ file .env ─────────────────────────────────────────
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Bắt buộc .NET đọc thêm cấu hình từ Environment Variables để đè lên appsettings.json
builder.Configuration.AddEnvironmentVariables();

// ── 2. Database Configuration ──────────────────────────────────────────────────
// Chuỗi kết nối sẽ tự động lấy từ biến ConnectionStrings__DefaultConnection trong .env
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ── 3. Cloudinary Configuration ────────────────────────────────────────────────
// Tự động map các biến Cloudinary__CloudName, Cloudinary__ApiKey... từ .env
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// ── 4. Dependency Injection (DI) ──────────────────────────────────────────────
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();

// ── 5. Cấu hình API & Controller ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Giới hạn kích thước file upload (tối đa 10 MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// ── 6. Swagger / OpenAPI Configuration ─────────────────────────────────────────
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
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Cấu hình JWT Bearer Token cho Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {token}"
    });

    c.UseInlineDefinitionsForEnums();
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

var app = builder.Build();

// ── 7. Seed dữ liệu test ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();

        // Kiểm tra nếu chưa có dữ liệu Citizen thì thêm mới
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
    catch (Exception ex)
    {
        // Ghi log nếu lỗi kết nối hoặc seed data
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi seed dữ liệu vào Database.");
    }
}

// ── 8. Global Exception Handler ────────────────────────────────────────────────
app.UseExceptionHandler(err => err.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode = ex switch
    {
        KeyNotFoundException => StatusCodes.Status404NotFound,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        ArgumentException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    await ctx.Response.WriteAsJsonAsync(new GarbageCollection.Common.DTOs.ApiResponse<object>
    {
        Success = false,
        Data = null,
        Message = ex?.Message ?? "Đã xảy ra lỗi không xác định."
    });
}));

// ── 9. Middleware Pipeline ─────────────────────────────────────────────────────
// Swagger luôn bật để tiện test
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarbageCollection Waste API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

// Khởi chạy ứng dụng
app.Run();
