# 📋 Deployment Checklist for Render

## ✅ Pre-Deployment

- [ ] Code đã được commit và push lên GitHub
- [ ] Đã test API trên local environment
- [ ] Đã test với PostgreSQL (sử dụng Docker Compose)
- [ ] Tất cả migrations đã được tạo
- [ ] Environment variables đã được prepare

## 🗄️ Step 1: Create PostgreSQL Database

1. **Login to Render Dashboard**

   - Go to https://dashboard.render.com

2. **Create New PostgreSQL Database**
   - Click `New +` → `PostgreSQL`
   - Fill in details:
     - Name: `snapdi-database`
     - Database: `snapdi_db`
     - User: `snapdi_user`
     - Region: `Singapore` (or closest)
     - Plan: `Free` (or your choice)
3. **Save Connection Info**
   - [ ] Copy `Internal Database URL` (for web service)
   - [ ] Copy `External Database URL` (for local migrations)
   - Format: `postgresql://user:password@host:5432/database`

## 🚀 Step 2: Create Web Service

1. **Create New Web Service**

   - Click `New +` → `Web Service`
   - Connect GitHub repository

2. **Basic Configuration**

   - [ ] Name: `snapdi-api`
   - [ ] Region: `Singapore` (same as database)
   - [ ] Branch: `kietnt` (your branch)
   - [ ] Root Directory: `Snapdi_App_BE/Snapdi`
   - [ ] Environment: `Docker`
   - [ ] Dockerfile Path: `./Dockerfile`

3. **Environment Variables** ⚠️ IMPORTANT

   Add these variables:

   ```bash
   # Database (paste Internal Database URL)
   CONNECTION_STRING=postgresql://user:pass@host/db?ssl=true

   # JWT Settings (CHANGE THESE!)
   JWT_KEY=your-super-secret-key-at-least-32-characters-long-please-change-this
   JWT_ISSUER=SnapdiAPI
   JWT_AUDIENCE=SnapdiClient
   JWT_EXPIRATION_HOURS=24

   # App Settings
   APP_BASE_URL=https://snapdi-api.onrender.com
   ASPNETCORE_ENVIRONMENT=Production

   # Email (Optional - only if you use email features)
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-app-password
   FROM_EMAIL=your-email@gmail.com
   FROM_NAME=Snapdi Team
   ```

4. **Deploy Settings**
   - [ ] Auto-Deploy: `Yes` (recommended)
   - [ ] Docker Command: Leave empty
   - Click `Create Web Service`

## 📊 Step 3: Run Database Migrations

**Option A: From Local Machine (Recommended)**

```powershell
# Set connection string (use External Database URL)
$env:CONNECTION_STRING="postgresql://user:pass@host/db"

# Navigate to project
cd Snapdi.Api

# Run migrations
dotnet ef database update --project ../Snapdi.Repositories

# Or use migration script
cd ..
.\migrate.ps1
# Choose option 2 (Apply migrations)
```

**Option B: Import SQL Dump**

1. Export schema from SQL Server
2. Convert to PostgreSQL syntax
3. Import via Render Dashboard or pgAdmin

## 🔍 Step 4: Verify Deployment

- [ ] Check Logs for errors
  - Render Dashboard → Your Service → Logs tab
- [ ] Test API endpoints
  - Open: `https://snapdi-api.onrender.com/swagger`
  - Should see Swagger UI
- [ ] Test Authentication
  - Try register/login endpoints
  - Verify JWT token generation
- [ ] Test Database Connection
  - Try endpoints that read/write data
  - Check if data persists

## 🔧 Step 5: Update Client Apps

Update API URLs in client applications:

**Mobile App (Flutter)**

```dart
// lib/config/api_config.dart
static const String baseUrl = 'https://snapdi-api.onrender.com';
```

**Web App (React/TypeScript)**

```typescript
// src/config/api.ts
export const API_BASE_URL = "https://snapdi-api.onrender.com";
```

## ⚠️ Important Notes

### Free Plan Limitations

- **Database**:
  - ✅ 1GB storage
  - ⚠️ Auto-deleted after 90 days of inactivity
  - ✅ Automatic backups
- **Web Service**:
  - ⚠️ Sleeps after 15 minutes of inactivity
  - ⚠️ Cold start: 30-60 seconds wake up time
  - ✅ 750 hours/month free

### Security Checklist

- [ ] JWT_KEY is at least 32 characters
- [ ] JWT_KEY is unique and secure (not the example)
- [ ] Database password is strong
- [ ] CORS is properly configured
- [ ] HTTPS is enabled (automatic on Render)
- [ ] .env file is NOT committed to Git

### Performance Tips

- [ ] Enable connection pooling (already configured in EF Core)
- [ ] Add Redis cache for frequent queries (future enhancement)
- [ ] Enable gzip compression (already enabled in ASP.NET Core)
- [ ] Monitor API response times in Render Dashboard

## 🐛 Troubleshooting

### Build Failed

**Problem**: Docker build fails on Render

**Solutions**:

- [ ] Check Dockerfile path is correct: `./Dockerfile`
- [ ] Verify root directory: `Snapdi_App_BE/Snapdi`
- [ ] Check build logs for specific errors
- [ ] Ensure all .csproj files have correct references

### Application Error

**Problem**: Service deployed but shows error

**Solutions**:

- [ ] Check all environment variables are set
- [ ] Verify CONNECTION_STRING format is correct
- [ ] Check JWT_KEY is at least 32 characters
- [ ] Review logs for specific error messages

### Database Connection Failed

**Problem**: Cannot connect to PostgreSQL

**Solutions**:

- [ ] Use `Internal Database URL` (not External)
- [ ] Ensure `SSL Mode=Require` in connection string
- [ ] Verify database is in same region as web service
- [ ] Check database is running (not suspended)

### Migrations Failed

**Problem**: Cannot run migrations

**Solutions**:

- [ ] Use `External Database URL` for local migrations
- [ ] Ensure EF Core tools are installed: `dotnet tool install --global dotnet-ef`
- [ ] Check connection string has proper SSL settings
- [ ] Try creating new PostgreSQL-specific migrations

### Cold Start Slow

**Problem**: First request takes 30-60 seconds

**Solutions**:

- ✅ This is normal for free plan (service sleeps)
- 💡 Upgrade to paid plan to prevent sleeping
- 💡 Use uptime monitoring service to keep alive
- 💡 Show loading state in client apps

### API Returns 502/503

**Problem**: Gateway errors

**Solutions**:

- [ ] Check if service is deploying (wait for completion)
- [ ] Verify service didn't crash (check logs)
- [ ] Ensure port 8080 is exposed correctly
- [ ] Check health check endpoint

## 📈 Post-Deployment Monitoring

### Daily Checks

- [ ] API is responding
- [ ] No critical errors in logs
- [ ] Database storage usage

### Weekly Checks

- [ ] Review error logs
- [ ] Check API response times
- [ ] Monitor database performance
- [ ] Update dependencies if needed

### Monthly Checks

- [ ] Review security updates
- [ ] Backup database
- [ ] Clean up old logs
- [ ] Review and optimize slow queries

## 🔄 Continuous Deployment

Once set up, deployment is automatic:

1. **Make changes** in your code
2. **Commit and push** to GitHub
   ```bash
   git add .
   git commit -m "Your changes"
   git push origin kietnt
   ```
3. **Render auto-deploys** from the branch
4. **Monitor logs** during deployment
5. **Verify** the changes work

## 📞 Support Resources

- **Render Docs**: https://render.com/docs
- **Render Community**: https://community.render.com
- **EF Core Docs**: https://docs.microsoft.com/ef/core
- **PostgreSQL Docs**: https://www.postgresql.org/docs

## ✨ Success Criteria

Deployment is successful when:

- ✅ Swagger UI loads at `https://your-app.onrender.com/swagger`
- ✅ Register endpoint creates new user
- ✅ Login endpoint returns JWT token
- ✅ Protected endpoints work with JWT
- ✅ Data persists in PostgreSQL
- ✅ No errors in Render logs
- ✅ Client apps can connect to API

---

**🎉 Congratulations! Your Snapdi Backend API is now live on Render!**
