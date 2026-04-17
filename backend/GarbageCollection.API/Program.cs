using CloudinaryDotNet;
using DotNetEnv;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using GarbageCollection.Common.Settings;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using GarbageCollection.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
<<<<<<< HEAD
using System.Text;
using System.Text.Json;
=======
>>>>>>> 98025b8 (feat: add respone format api)
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using DotNetEnv;

// ── 1. Nạp biến môi trường từ file .env ─────────────────────────────────────────
Env.Load();

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// ─────────────────────────────────────────────
// 1. ENV CONFIG
// ─────────────────────────────────────────────
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// ─────────────────────────────────────────────
// 2. DB CONFIG
// ─────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─────────────────────────────────────────────
// 3. SETTINGS
// ─────────────────────────────────────────────
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] 
    ?? throw new Exception("Jwt:SecretKey missing");

<<<<<<< HEAD
// ─────────────────────────────────────────────
// 4. CORE SERVICES
// ─────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();

// Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocalAuthService, LocalAuthService>();
builder.Services.AddScoped<ILocalLoginService, LocalLoginService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IVerifyEmailService, VerifyEmailService>();

builder.Services.AddSingleton<JwtHelper>();

// ─────────────────────────────────────────────
// 5. AUTH - JWT
// ─────────────────────────────────────────────
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────
// 6. CORS
// ─────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://ecoconnect-citizen.lovable.app",
                "https://eco-connect-admin-re.lovable.app",
                "https://eco-connect-collector.lovable.app",
                "https://eco-conect-landing-page.lovable.app",
                "https://collect-garbage-production.up.railway.app",
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─────────────────────────────────────────────
// 7. CONTROLLERS
// ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// Upload limit
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// ─────────────────────────────────────────────
// 8. SWAGGER
// ─────────────────────────────────────────────
=======
// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
>>>>>>> 98025b8 (feat: add respone format api)
=======
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

// ── 4. CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                // Lovable frontends
                "https://ecoconnect-citizen.lovable.app",
                "https://eco-connect-admin-re.lovable.app",
                "https://eco-connect-collector.lovable.app",
                "https://eco-conect-landing-page.lovable.app",
                // Railway backend (Swagger UI)
                "https://collect-garbage-production.up.railway.app",
                // Local dev
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── 5. Dependency Injection (DI) ──────────────────────────────────────────────
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();

// ── 5. Cấu hình API & Controller ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// Giới hạn kích thước file upload (tối đa 10 MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// ── 6. Swagger / OpenAPI Configuration ─────────────────────────────────────────
>>>>>>> 277178b (feat: add evn)
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GarbageCollection API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
<<<<<<< HEAD
        c.IncludeXmlComments(xmlPath);

=======
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Cấu hình JWT Bearer Token cho Swagger
>>>>>>> 277178b (feat: add evn)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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

<<<<<<< HEAD
// ─────────────────────────────────────────────
// 9. BUILD APP
// ─────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────
// 10. SEED DATA
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
=======
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
var exceptionJsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Converters = { new JsonStringEnumConverter() }
};

app.UseExceptionHandler(err => err.Run(async ctx =>
>>>>>>> 277178b (feat: add evn)
{
<<<<<<< HEAD
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

// ─────────────────────────────────────────────
// 11. EXCEPTION HANDLER
// ─────────────────────────────────────────────
app.UseExceptionHandler(err => err.Run(async context =>
{
    var ex = context.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsJsonAsync(new
=======
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    ctx.Response.ContentType = "application/json";
    ctx.Response.StatusCode = ex switch
>>>>>>> 98025b8 (feat: add respone format api)
    {
        KeyNotFoundException      => StatusCodes.Status404NotFound,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        ArgumentException         => StatusCodes.Status400BadRequest,
        _                         => StatusCodes.Status500InternalServerError
    };

    var errorCode = ctx.Response.StatusCode switch
    {
        404 => "NOT_FOUND",
        400 => "BAD_REQUEST",
        _   => "INTERNAL_SERVER_ERROR"
    };

    await ctx.Response.WriteAsJsonAsync(
        new GarbageCollection.Common.DTOs.ApiResponse<object>
        {
            Status  = "failed",
            Message = ex?.Message ?? "Đã xảy ra lỗi không xác định.",
            Data    = null,
            Error   = new GarbageCollection.Common.DTOs.ApiError
            {
                Code        = errorCode,
                Description = ex?.Message ?? "Unknown error"
            }
        },
        exceptionJsonOptions);
}));

<<<<<<< HEAD
// ─────────────────────────────────────────────
// 12. MIDDLEWARE PIPELINE
// ─────────────────────────────────────────────
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

=======
// ── 9. Middleware Pipeline ─────────────────────────────────────────────────────
// Cần cho Railway reverse proxy — giúp Swagger detect đúng scheme HTTPS
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowFrontend");

// Swagger luôn bật để tiện test
>>>>>>> 277178b (feat: add evn)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarbageCollection API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

<<<<<<< HEAD
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
=======
// Khởi chạy ứng dụng
app.Run();
>>>>>>> 277178b (feat: add evn)
