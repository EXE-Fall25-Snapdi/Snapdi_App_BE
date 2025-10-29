# Environment Variables Template for Snapdi API

## Overview

This document lists all environment variables required to run the Snapdi Backend API. Use this as a reference when configuring your deployment on DigitalOcean, local development, or other platforms.

---

## Required Environment Variables

### 1. Database Configuration (Supabase PostgreSQL)

```bash
CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

**Description**: PostgreSQL connection string for Supabase database

**How to get**:
1. Go to Supabase Dashboard → Settings → Database
2. Copy the connection parameters
3. Format as shown above with `SSL Mode=Require`

**Example for local PostgreSQL**:
```bash
CONNECTION_STRING=Host=localhost;Port=5432;Database=snapdi_db;Username=postgres;Password=your_password
```

---

### 2. JWT Authentication Settings

#### JWT_KEY (Required, Secret)
```bash
JWT_KEY=your-super-secret-jwt-key-minimum-32-characters-long-for-security
```

**Description**: Secret key for JWT token signing (minimum 32 characters)

**How to generate**:
```bash
# Using OpenSSL
openssl rand -base64 32

# Using PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | % {[char]$_})
```

**Security**: Never commit this to version control!

#### JWT_ISSUER
```bash
JWT_ISSUER=SnapdiAPI
```

**Description**: JWT token issuer identifier

#### JWT_AUDIENCE
```bash
JWT_AUDIENCE=SnapdiClient
```

**Description**: JWT token audience identifier

#### JWT_EXPIRATION_HOURS
```bash
JWT_EXPIRATION_HOURS=24
```

**Description**: JWT token expiration time in hours (default: 24)

---

### 3. Application Configuration

#### APP_BASE_URL
```bash
# Local development
APP_BASE_URL=http://localhost:8080

# Production (DigitalOcean)
APP_BASE_URL=https://your-app.ondigitalocean.app

# Production (Custom domain)
APP_BASE_URL=https://api.snapdi.com
```

**Description**: Base URL of your API (used in email links, webhooks, etc.)

**Note**: On DigitalOcean, you can use `${APP_URL}` which auto-resolves to your app URL

#### ASPNETCORE_ENVIRONMENT
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development

# Production
ASPNETCORE_ENVIRONMENT=Production

# Staging
ASPNETCORE_ENVIRONMENT=Staging
```

**Description**: ASP.NET Core environment setting

**Impact**:
- `Development`: Detailed error pages, Swagger enabled
- `Production`: Minimal error info, optimized performance
- `Staging`: Testing environment before production

#### ASPNETCORE_URLS (Optional)
```bash
ASPNETCORE_URLS=http://+:8080
```

**Description**: URLs the application listens on (usually set in Dockerfile)

---

### 4. Cloudinary Configuration (Image Upload Service)

#### CLOUDINARY_CLOUD_NAME (Required)
```bash
CLOUDINARY_CLOUD_NAME=your_cloud_name
```

**How to get**: Sign up at https://cloudinary.com → Dashboard → Cloud name

#### CLOUDINARY_API_KEY (Required, Secret)
```bash
CLOUDINARY_API_KEY=123456789012345
```

**How to get**: Cloudinary Dashboard → API Keys → API Key

#### CLOUDINARY_API_SECRET (Required, Secret)
```bash
CLOUDINARY_API_SECRET=your_api_secret_here
```

**How to get**: Cloudinary Dashboard → API Keys → API Secret

#### CLOUDINARY_UPLOAD_PRESET (Optional)
```bash
CLOUDINARY_UPLOAD_PRESET=snapdi_default
```

**Description**: Cloudinary upload preset (default: `snapdi_default`)

**Setup**:
1. Go to Cloudinary → Settings → Upload
2. Create upload preset named `snapdi_default`
3. Set signing mode to "Signed" for security

---

### 5. Email/SMTP Configuration (for verification emails)

#### SMTP_HOST (Required)
```bash
# Gmail
SMTP_HOST=smtp.gmail.com

# SendGrid
SMTP_HOST=smtp.sendgrid.net

# Mailgun
SMTP_HOST=smtp.mailgun.org

# Amazon SES (US East)
SMTP_HOST=email-smtp.us-east-1.amazonaws.com
```

**Description**: SMTP server hostname

#### SMTP_PORT (Required)
```bash
SMTP_PORT=587
```

**Description**: SMTP server port (usually 587 for TLS or 465 for SSL)

#### SMTP_USERNAME (Required, Secret)
```bash
SMTP_USERNAME=your-email@gmail.com
```

**Description**: SMTP authentication username

**For Gmail**: Your Gmail email address

#### SMTP_PASSWORD (Required, Secret)
```bash
SMTP_PASSWORD=your_app_password_here
```

**Description**: SMTP authentication password

**For Gmail**:
1. Enable 2-Factor Authentication
2. Go to Google Account → Security → 2-Step Verification
3. Scroll to "App passwords"
4. Generate password for "Mail"
5. Use the 16-character password (without spaces)

**For SendGrid**: Use your SendGrid API key as password (username: "apikey")

#### FROM_EMAIL (Required)
```bash
FROM_EMAIL=noreply@snapdi.com
```

**Description**: Email address shown as sender

#### FROM_NAME (Optional)
```bash
FROM_NAME=Snapdi Team
```

**Description**: Name shown as sender (default: "Snapdi Team")

---

## Complete Configuration Examples

### Local Development (.env file)

```bash
# Database (Local PostgreSQL)
CONNECTION_STRING=Host=localhost;Port=5432;Database=snapdi_db;Username=postgres;Password=postgres123

# JWT
JWT_KEY=local-development-jwt-key-32chars-minimum-required-here
JWT_ISSUER=SnapdiAPI
JWT_AUDIENCE=SnapdiClient
JWT_EXPIRATION_HOURS=24

# App
APP_BASE_URL=http://localhost:8080
ASPNETCORE_ENVIRONMENT=Development

# Cloudinary (Test account)
CLOUDINARY_CLOUD_NAME=test-cloud
CLOUDINARY_API_KEY=123456789012345
CLOUDINARY_API_SECRET=test_api_secret_here
CLOUDINARY_UPLOAD_PRESET=snapdi_default

# Email (Gmail)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=test@gmail.com
SMTP_PASSWORD=abcd efgh ijkl mnop
FROM_EMAIL=test@gmail.com
FROM_NAME=Snapdi Team
```

---

### Production (DigitalOcean Environment Variables)

Set these in DigitalOcean App Platform → Settings → Environment Variables:

```bash
# Database (Supabase)
CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true
# Mark as: Encrypted ✓

# JWT
JWT_KEY=production-super-secret-jwt-key-at-least-32-characters-long-random-secure
# Mark as: Encrypted ✓
JWT_ISSUER=SnapdiAPI
JWT_AUDIENCE=SnapdiClient
JWT_EXPIRATION_HOURS=24

# App
APP_BASE_URL=${APP_URL}
# DigitalOcean auto-resolves this to your app URL
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Cloudinary (Production account)
CLOUDINARY_CLOUD_NAME=snapdi-production
CLOUDINARY_API_KEY=987654321098765
# Mark as: Encrypted ✓
CLOUDINARY_API_SECRET=production_api_secret_here
# Mark as: Encrypted ✓
CLOUDINARY_UPLOAD_PRESET=snapdi_default

# Email (Production Gmail or SendGrid)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=noreply@snapdi.com
# Mark as: Encrypted ✓
SMTP_PASSWORD=prod_app_password_here
# Mark as: Encrypted ✓
FROM_EMAIL=noreply@snapdi.com
FROM_NAME=Snapdi Team
```

---

### Docker Compose (docker-compose.yml)

```yaml
environment:
  CONNECTION_STRING: "Host=postgres;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123"
  JWT_KEY: "docker-local-jwt-key-minimum-32-characters-required"
  JWT_ISSUER: "SnapdiAPI"
  JWT_AUDIENCE: "SnapdiClient"
  JWT_EXPIRATION_HOURS: "24"
  APP_BASE_URL: "http://localhost:8080"
  ASPNETCORE_ENVIRONMENT: "Development"
  CLOUDINARY_CLOUD_NAME: "test-cloud"
  CLOUDINARY_API_KEY: "123456789012345"
  CLOUDINARY_API_SECRET: "test_secret"
  CLOUDINARY_UPLOAD_PRESET: "snapdi_default"
  SMTP_HOST: "smtp.gmail.com"
  SMTP_PORT: "587"
  SMTP_USERNAME: "test@gmail.com"
  SMTP_PASSWORD: "test_password"
  FROM_EMAIL: "test@gmail.com"
  FROM_NAME: "Snapdi Team"
```

---

## How Environment Variables are Used

The application reads environment variables in this order:

1. **Environment variables** (highest priority)
2. `.env` file (local development)
3. `appsettings.json` (fallback)

See `Program.cs` for implementation:

```csharp
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                      builder.Configuration.GetConnectionString("DefaultConnection");
```

---

## Security Best Practices

### ✅ DO:
- Use long, random strings for JWT_KEY (≥32 characters)
- Mark sensitive variables as "Encrypted" in DigitalOcean
- Use different credentials for dev/staging/production
- Rotate secrets regularly (every 3-6 months)
- Use App Passwords for Gmail (not your main password)
- Enable 2FA on all service accounts

### ❌ DON'T:
- Commit `.env` file to version control (add to `.gitignore`)
- Use weak or predictable JWT keys
- Reuse production credentials in development
- Share credentials in plain text (Slack, email, etc.)
- Use your personal Gmail password (always use App Password)

---

## Validation

The application validates required configuration on startup:

```csharp
// JWT Key validation
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY is required");
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT_KEY must be at least 32 characters long");
}
```

If validation fails, check logs for specific error messages.

---

## Troubleshooting

### "JWT_KEY is required"
→ Set the `JWT_KEY` environment variable with at least 32 characters

### "Cannot connect to database"
→ Check `CONNECTION_STRING` format and credentials
→ For Supabase, ensure `SSL Mode=Require` is included

### "SMTP authentication failed"
→ For Gmail: Use App Password, not regular password
→ Ensure 2FA is enabled on Gmail account

### "Cloudinary upload failed"
→ Check API credentials are correct
→ Ensure upload preset exists and is configured

---

## Quick Reference

| Variable | Required | Secret | Default | Where to get |
|----------|----------|--------|---------|--------------|
| CONNECTION_STRING | ✓ | ✓ | - | Supabase Dashboard |
| JWT_KEY | ✓ | ✓ | - | Generate random |
| JWT_ISSUER | ✓ | - | SnapdiAPI | Choose any |
| JWT_AUDIENCE | ✓ | - | SnapdiClient | Choose any |
| JWT_EXPIRATION_HOURS | - | - | 24 | Configure as needed |
| APP_BASE_URL | ✓ | - | - | Your app URL |
| ASPNETCORE_ENVIRONMENT | - | - | Production | Dev/Staging/Prod |
| CLOUDINARY_CLOUD_NAME | ✓ | - | - | Cloudinary Dashboard |
| CLOUDINARY_API_KEY | ✓ | ✓ | - | Cloudinary Dashboard |
| CLOUDINARY_API_SECRET | ✓ | ✓ | - | Cloudinary Dashboard |
| CLOUDINARY_UPLOAD_PRESET | - | - | snapdi_default | Cloudinary Settings |
| SMTP_HOST | ✓ | - | - | Email provider |
| SMTP_PORT | ✓ | - | 587 | Email provider |
| SMTP_USERNAME | ✓ | ✓ | - | Email account |
| SMTP_PASSWORD | ✓ | ✓ | - | Email account/App password |
| FROM_EMAIL | ✓ | - | - | Your email |
| FROM_NAME | - | - | Snapdi Team | Choose any |

---

## Related Documentation

- `DIGITALOCEAN_DEPLOYMENT_GUIDE.md` - Full deployment guide
- `SUPABASE_DATABASE_SETUP.md` - Database setup guide
- `DIGITALOCEAN_QUICKSTART.md` - Quick start guide
- `digitalocean-app.yaml` - App Platform specification with all variables

---

## Support

For help with environment variables:
- Check application logs for validation errors
- Verify all required variables are set
- Ensure sensitive values are properly encrypted
- Test connection to external services (database, email, Cloudinary)

---

Last updated: 2025-10-29


