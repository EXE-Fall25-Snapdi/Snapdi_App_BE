using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Snapdi.Api.Services;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Repositories;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Interfaces.Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using Snapdi.Services.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Get configuration from environment variables
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                      builder.Configuration.GetConnectionString("DefaultConnection");

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ??
            builder.Configuration["JWT:Key"];

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
               builder.Configuration["JWT:Issuer"];

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                 builder.Configuration["JWT:Audience"];

var jwtExpirationHours = Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS") ??
                       builder.Configuration["JWT:ExpirationHours"];

var appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ??
                builder.Configuration["App:BaseUrl"];

var cloudinaryCloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ??
                         builder.Configuration["Cloudinary:CloudName"];

var cloudinaryApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ??
                      builder.Configuration["Cloudinary:ApiKey"];

var cloudinaryApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ??
                         builder.Configuration["Cloudinary:ApiSecret"];

var cloudinaryUploadPreset = Environment.GetEnvironmentVariable("CLOUDINARY_UPLOAD_PRESET") ??
                            builder.Configuration["Cloudinary:UploadPreset"] ?? "snapdi_default";

var payOSClientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID") ??
                   builder.Configuration["PayOS:ClientId"];

var payOSApiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY") ??
                 builder.Configuration["PayOS:ApiKey"];

var payOSChecksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY") ??
                      builder.Configuration["PayOS:ChecksumKey"];

var payOSReturnUrl = Environment.GetEnvironmentVariable("PAYOS_RETURN_URL") ??
                            builder.Configuration["PayOS:ReturnUrl"];

var payOSCancelUrl = Environment.GetEnvironmentVariable("PAYOS_CANCEL_URL") ??
                            builder.Configuration["PayOS:CancelUrl"];

// Validate required configuration
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY is required. Please set it in .env file or configuration.");
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT_KEY must be at least 32 characters long for security.");
}

// Add CORS services with SignalR support
// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Allow JWT over WebSockets for SignalR using access_token query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Add DbContext
builder.Services.AddDbContext<SnapdiDbV2Context>(options =>
    options.UseSqlServer(connectionString, x => x.UseNetTopologySuite()));

// Configure settings through DI
builder.Services.Configure<AppSettings>(options =>
{
    options.BaseUrl = appBaseUrl ?? "https://localhost:7000";
});

builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
    options.SmtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
    options.SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
    options.FromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "";
    options.FromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "Snapdi Team";
});

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtKey;
    options.Issuer = jwtIssuer ?? "";
    options.Audience = jwtAudience ?? "";
    options.ExpirationHours = int.Parse(jwtExpirationHours ?? "1");
});

builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = cloudinaryCloudName ?? "";
    options.ApiKey = cloudinaryApiKey ?? "";
    options.ApiSecret = cloudinaryApiSecret ?? "";
    options.UploadPreset = cloudinaryUploadPreset;
    options.FolderPath = "snapdi";
    options.UseSignedUpload = true;
});

builder.Services.Configure<PayOSSettings>(options =>
{
    options.payOSClientId = payOSClientId ?? "";
    options.payOSApiKey = payOSApiKey ?? "";
    options.payOSChecksumKey = payOSChecksumKey ?? "";
    options.payOSReturnUrl = payOSReturnUrl ?? "";
    options.payOSCancelUrl = payOSCancelUrl ?? "";
});

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IKeywordRepository, KeywordRepository>();
builder.Services.AddScoped<IPhotographerProfileRepository, PhotographerProfileRepository>();
builder.Services.AddScoped<IPhotoPortfolioRepository, PhotoPortfolioRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IVoucherUsageRepository, VoucherUsageRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingStatusRepository, BookingStatusRepository>();
builder.Services.AddScoped<IStyleRepository, StyleRepository>();
builder.Services.AddScoped<IPhotographerStyleRepository, PhotographerStyleRepository>();
builder.Services.AddScoped<IPhotoTypeRepository, PhotoTypeRepository>();
builder.Services.AddScoped<IPhotographerPhotoTypeRepository, PhotographerPhotoTypeRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentStatusRepository, PaymentStatusRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IKeywordService, KeywordService>();
builder.Services.AddScoped<IPhotoPortfolioService, PhotoPortfolioService>();
builder.Services.AddSingleton<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IVoucherUsageService, VoucherUsageService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IStyleService, StyleService>();
builder.Services.AddScoped<IPhotographerStyleService, PhotographerStyleService>();
builder.Services.AddScoped<IPhotoTypeService, PhotoTypeService>();
builder.Services.AddScoped<IPhotographerPhotoTypeService, PhotographerPhotoTypeService>();
builder.Services.AddScoped<IPaymentsService, PaymentService>();
builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// SignalR for realtime features with extended options
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configure Swagger/OpenAPI with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Snapdi API",
        Version = "v1",
        Description = "API for Snapdi Photography Platform with Real-time Booking Updates",
        Contact = new OpenApiContact
        {
            Name = "Snapdi Team",
            Email = "support@snapdi.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Snapdi API V1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelExpandDepth(2);
        c.DefaultModelsExpandDepth(-1);
        c.DisplayOperationId();
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

// Enable CORS (must be before Authentication and Authorization)
app.UseCors("AllowFlutterApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<Snapdi.Api.Hubs.ChatHub>("/hubs/chat");
app.MapHub<Snapdi.Api.Hubs.BookingHub>("/hubs/booking");

app.Run();
