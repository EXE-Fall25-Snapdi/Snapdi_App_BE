using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Snapdi.Repositories.Models;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Repositories;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Services;
using Snapdi.Services.Models;
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

var appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? 
                builder.Configuration["App:BaseUrl"];

// Validate required configuration
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY is required. Please set it in .env file or configuration.");
}

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
builder.Services.AddDbContext<Snapdi_DB_v1_Context>(options =>
    options.UseSqlServer(connectionString));

// Add configuration for app settings
builder.Services.Configure<AppSettings>(options =>
{
    options.BaseUrl = appBaseUrl ?? "https://localhost:7000";
});

// Add configuration for email settings
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
    options.SmtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
    options.SmtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
    options.SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
    options.FromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? "";
    options.FromName = Environment.GetEnvironmentVariable("FROM_NAME") ?? "Snapdi Team";
});

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
