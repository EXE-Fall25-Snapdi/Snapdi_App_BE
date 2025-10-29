# Environment Variables Template

Copy these variables to your `.env` file in `Snapdi.Api/.env` directory.

## Required Environment Variables

```bash
# =============================================
# Database Configuration (Supabase PostgreSQL)
# =============================================
CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true

# =============================================
# JWT Authentication Settings
# =============================================
# IMPORTANT: Generate a secure key of at least 32 characters for production
# Example to generate: openssl rand -base64 32
JWT_KEY=your-super-secret-jwt-key-minimum-32-characters-long-for-security-please-change-this
JWT_ISSUER=SnapdiAPI
JWT_AUDIENCE=SnapdiClient
JWT_EXPIRATION_HOURS=24

# =============================================
# Application Base URL
# =============================================
# For Cloud Run: will be https://your-service-name-xxxxxxxxxx-xx.a.run.app
# Update this after deployment
APP_BASE_URL=https://localhost:7000

# =============================================
# Cloudinary Image Upload Settings
# =============================================
# Get these from https://cloudinary.com/console
CLOUDINARY_CLOUD_NAME=your_cloudinary_cloud_name
CLOUDINARY_API_KEY=your_cloudinary_api_key
CLOUDINARY_API_SECRET=your_cloudinary_api_secret
CLOUDINARY_UPLOAD_PRESET=snapdi_default

# =============================================
# Email/SMTP Configuration (Optional)
# =============================================
# Gmail example: smtp.gmail.com, port 587
# Use App Password if using Gmail with 2FA
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=noreply@snapdi.com
FROM_NAME=Snapdi Team

# =============================================
# ASP.NET Core Environment
# =============================================
# Use "Production" for Cloud Run deployment
ASPNETCORE_ENVIRONMENT=Production
```

## Instructions

1. **Create .env file**: Copy the above content to `Snapdi.Api/.env`
2. **Update Supabase**: Connection string is already configured
3. **Generate JWT Key**: Run `openssl rand -base64 32` to generate a secure key
4. **Add Cloudinary credentials**: Fill in your Cloudinary account details
5. **Configure Email** (Optional): Add your SMTP settings for email notifications
6. **Update APP_BASE_URL**: After deploying to Cloud Run, update this with your service URL

## For Cloud Run Deployment

These environment variables will be set directly in Cloud Run service configuration (not using .env file):

- Use Google Secret Manager for sensitive values (JWT_KEY, CLOUDINARY_API_SECRET, SMTP_PASSWORD)
- Set non-sensitive values directly as environment variables
- The deployment script will handle this automatically

