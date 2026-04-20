using CloudinaryDotNet;
using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using GarbageCollection.Common.Settings;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using GarbageCollection.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.MigrationsAssembly("GarbageCollection.DataAccess")));

// ── 2. Configuration Settings ────────────────────────────────────────────────
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? throw new Exception("Jwt:SecretKey missing");

// ── 3. Identity & Access (Cần thiết cho Cookie/Auth) ─────────────────────────
builder.Services.AddHttpContextAccessor(); // Quan trọng để truy cập HttpContext trong Service

// ── 4. Repositories & Services (DI) ──────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IWasteReportRepository, WasteReportRepository>();
builder.Services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();



builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IWasteReportService, WasteReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocalAuthService, LocalAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IVerifyEmailService, VerifyEmailService>();
builder.Services.AddScoped<ILocalLoginService, LocalLoginService>();

// ── 5. JWT Authentication ────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set true khi lên production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Loại bỏ thời gian trễ mặc định (5p)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Ưu tiên lấy token từ Cookie (dành cho Web FE)
                if (context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                // Nếu không có cookie, JwtBearer sẽ tự động tìm trong header "Authorization: Bearer ..."
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── 6. CORS (Cực kỳ quan trọng khi dùng Cookie) ──────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000") // Đảm bảo khớp với URL của React/Next.js
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // BẮT BUỘC có cái này để gửi/nhận Cookie
});

// ── 7. Controllers & Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GarbageCollection API", Version = "v1" });

    // Hỗ trợ XML Comment (đảm bảo file .xml tồn tại)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token: {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024);

var app = builder.Build();

// ── 8. Pipeline Middleware ───────────────────────────────────────────────────

// Seeding (Nên tách ra một class riêng nhưng tạm thời để đây cũng được)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.Migrate(); // Tự động chạy Migration nếu cần
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

app.UseExceptionHandler(err => err.Run(async context =>
{
    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";

    await context.Response.WriteAsJsonAsync(new
    {
        error = ex?.Message,
        detail = ex?.InnerException?.Message
    });
}));


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarbageCollection API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Thứ tự này KHÔNG ĐƯỢC SAI
app.UseAuthentication(); // Ai là người đang truy cập?
app.UseAuthorization();  // Người đó có quyền làm gì?

app.MapControllers();

app.Run();