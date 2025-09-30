using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Snapdi.Api.Services;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Repositories.Repositories;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using Snapdi.Services.Services;
using System;
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

// Validate required configuration
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY is required. Please set it in .env file or configuration.");
}

// Validate JWT key length (should be at least 32 characters for HMAC SHA256)
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT_KEY must be at least 32 characters long for security.");
}

Console.WriteLine($"JWT Configuration: Key Length={jwtKey.Length}, Issuer={jwtIssuer}, Audience={jwtAudience}");

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

});

// Add DbContext
builder.Services.AddDbContext<SnapdiDbV2Context>(options =>
    options.UseSqlServer(connectionString));

// Configure settings through DI (single source of truth)
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

// Configure JWT settings (single source - no duplication)
builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtKey;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
    options.ExpirationHours = int.Parse(jwtExpirationHours ?? "1");
});

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IKeywordRepository, KeywordRepository>();

//// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IKeywordService, KeywordService>();
builder.Services.AddScoped<JwtService>(); // JwtService will receive JwtSettings via DI

builder.Services.AddControllers();

// Configure Swagger/OpenAPI with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Snapdi API", 
        Version = "v1",
        Description = "API for Snapdi Photography Platform",
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
