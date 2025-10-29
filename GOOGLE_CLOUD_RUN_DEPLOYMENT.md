# Google Cloud Run Deployment Guide - Snapdi API

Complete guide for deploying Snapdi .NET 8.0 API to Google Cloud Run with Supabase PostgreSQL database.

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Step-by-Step Deployment](#step-by-step-deployment)
4. [Environment Variables Configuration](#environment-variables-configuration)
5. [Manual Deployment](#manual-deployment)
6. [Post-Deployment Configuration](#post-deployment-configuration)
7. [Monitoring and Logs](#monitoring-and-logs)
8. [Troubleshooting](#troubleshooting)
9. [Cost Optimization](#cost-optimization)

---

## Prerequisites

### ✅ Required

- [x] Google Cloud account with $300 trial credit
- [x] Supabase database configured and accessible
- [x] [Google Cloud SDK](https://cloud.google.com/sdk/docs/install) installed
- [x] [Docker Desktop](https://www.docker.com/products/docker-desktop) installed
- [x] PowerShell 5.1+ (Windows) or PowerShell Core (cross-platform)

### 🔧 Setup Checklist

1. **Google Cloud SDK Installation**
   ```powershell
   # Verify installation
   gcloud version
   
   # Login to Google Cloud
   gcloud auth login
   
   # List your projects
   gcloud projects list
   ```

2. **Docker Installation**
   ```powershell
   # Verify Docker is running
   docker --version
   docker ps
   ```

3. **Supabase Database**
   - ✅ Database created and running
   - ✅ Connection string available
   - ✅ Database schema initialized (run `Scripts/init-database-postgres.sql`)

---

## 🚀 Quick Start

### Option 1: Automated Deployment (Recommended)

1. **Navigate to project directory**
   ```powershell
   cd BE2\Snapdi_App_BE
   ```

2. **Configure environment variables**
   
   Create `.env` file in `Snapdi\Snapdi.Api\.env` with your settings:
   ```bash
   CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true
   JWT_KEY=your-secure-jwt-key-at-least-32-characters-long
   JWT_ISSUER=SnapdiAPI
   JWT_AUDIENCE=SnapdiClient
   JWT_EXPIRATION_HOURS=24
   CLOUDINARY_CLOUD_NAME=your_cloudinary_name
   CLOUDINARY_API_KEY=your_api_key
   CLOUDINARY_API_SECRET=your_api_secret
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-app-password
   FROM_EMAIL=noreply@snapdi.com
   FROM_NAME=Snapdi Team
   ```

3. **Run deployment script**
   ```powershell
   .\deploy-cloudrun.ps1 -ProjectId "your-gcp-project-id" -Region "asia-southeast1"
   ```

4. **Done!** Your API will be deployed and accessible at the Cloud Run URL.

---

## 📖 Step-by-Step Deployment

### Step 1: Google Cloud Project Setup

1. **Create or select a project**
   ```powershell
   # Create new project
   gcloud projects create snapdi-api-prod --name="Snapdi API Production"
   
   # Or list existing projects
   gcloud projects list
   
   # Set active project
   gcloud config set project YOUR_PROJECT_ID
   ```

2. **Enable billing** (required for Cloud Run)
   - Go to: https://console.cloud.google.com/billing
   - Link your $300 credit to the project

### Step 2: Enable Required APIs

```powershell
# Enable Cloud Run API
gcloud services enable run.googleapis.com

# Enable Artifact Registry API (for Docker images)
gcloud services enable artifactregistry.googleapis.com

# Enable Cloud Build API
gcloud services enable cloudbuild.googleapis.com
```

### Step 3: Create Artifact Registry Repository

```powershell
# Create repository for Docker images
gcloud artifacts repositories create snapdi-repo \
    --repository-format=docker \
    --location=asia-southeast1 \
    --description="Snapdi API Docker images"

# Configure Docker authentication
gcloud auth configure-docker asia-southeast1-docker.pkg.dev
```

### Step 4: Build Docker Image

```powershell
# Navigate to Snapdi directory
cd BE2\Snapdi_App_BE\Snapdi

# Build Docker image
# Replace YOUR_PROJECT_ID with your actual project ID
$PROJECT_ID = "your-gcp-project-id"
$IMAGE_NAME = "asia-southeast1-docker.pkg.dev/$PROJECT_ID/snapdi-repo/snapdi-api:latest"

docker build -t $IMAGE_NAME .
```

### Step 5: Push Image to Artifact Registry

```powershell
# Push image to Google Artifact Registry
docker push $IMAGE_NAME
```

### Step 6: Deploy to Cloud Run

```powershell
# Deploy with environment variables
gcloud run deploy snapdi-api \
    --image=$IMAGE_NAME \
    --platform=managed \
    --region=asia-southeast1 \
    --allow-unauthenticated \
    --port=8080 \
    --memory=512Mi \
    --cpu=1 \
    --min-instances=0 \
    --max-instances=10 \
    --timeout=300 \
    --set-env-vars="CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true,JWT_KEY=your-secure-key,JWT_ISSUER=SnapdiAPI,JWT_AUDIENCE=SnapdiClient,JWT_EXPIRATION_HOURS=24,ASPNETCORE_ENVIRONMENT=Production"
```

**Note**: For production, use Google Secret Manager for sensitive variables (see below).

### Step 7: Get Service URL

```powershell
# Get the deployed service URL
gcloud run services describe snapdi-api \
    --region=asia-southeast1 \
    --format="value(status.url)"
```

Example output: `https://snapdi-api-xxxxxxxxxx-xx.a.run.app`

---

## 🔐 Environment Variables Configuration

### Using Google Secret Manager (Recommended for Production)

1. **Create secrets for sensitive data**
   ```powershell
   # Create JWT Key secret
   echo -n "your-super-secret-jwt-key-32-chars-min" | gcloud secrets create jwt-key --data-file=-
   
   # Create Cloudinary API Secret
   echo -n "your-cloudinary-api-secret" | gcloud secrets create cloudinary-secret --data-file=-
   
   # Create SMTP Password
   echo -n "your-smtp-password" | gcloud secrets create smtp-password --data-file=-
   ```

2. **Grant Cloud Run access to secrets**
   ```powershell
   # Get the service account email
   $PROJECT_NUMBER = gcloud projects describe YOUR_PROJECT_ID --format="value(projectNumber)"
   $SERVICE_ACCOUNT = "$PROJECT_NUMBER-compute@developer.gserviceaccount.com"
   
   # Grant access to secrets
   gcloud secrets add-iam-policy-binding jwt-key \
       --member="serviceAccount:$SERVICE_ACCOUNT" \
       --role="roles/secretmanager.secretAccessor"
   
   gcloud secrets add-iam-policy-binding cloudinary-secret \
       --member="serviceAccount:$SERVICE_ACCOUNT" \
       --role="roles/secretmanager.secretAccessor"
   
   gcloud secrets add-iam-policy-binding smtp-password \
       --member="serviceAccount:$SERVICE_ACCOUNT" \
       --role="roles/secretmanager.secretAccessor"
   ```

3. **Update Cloud Run to use secrets**
   ```powershell
   gcloud run services update snapdi-api \
       --region=asia-southeast1 \
       --update-secrets=JWT_KEY=jwt-key:latest \
       --update-secrets=CLOUDINARY_API_SECRET=cloudinary-secret:latest \
       --update-secrets=SMTP_PASSWORD=smtp-password:latest
   ```

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `CONNECTION_STRING` | Supabase PostgreSQL connection | `Host=db.xxx.supabase.co;Database=postgres;...` |
| `JWT_KEY` | JWT signing key (32+ chars) | `your-super-secret-key-here` |
| `JWT_ISSUER` | JWT issuer | `SnapdiAPI` |
| `JWT_AUDIENCE` | JWT audience | `SnapdiClient` |
| `JWT_EXPIRATION_HOURS` | Token expiration time | `24` |
| `APP_BASE_URL` | Your Cloud Run URL | `https://snapdi-api-xxx.a.run.app` |
| `CLOUDINARY_CLOUD_NAME` | Cloudinary account name | `your-cloud-name` |
| `CLOUDINARY_API_KEY` | Cloudinary API key | `123456789012345` |
| `CLOUDINARY_API_SECRET` | Cloudinary API secret | `your-api-secret` |
| `CLOUDINARY_UPLOAD_PRESET` | Upload preset | `snapdi_default` |
| `SMTP_HOST` | SMTP server | `smtp.gmail.com` |
| `SMTP_PORT` | SMTP port | `587` |
| `SMTP_USERNAME` | Email username | `your-email@gmail.com` |
| `SMTP_PASSWORD` | Email password | `your-app-password` |
| `FROM_EMAIL` | Sender email | `noreply@snapdi.com` |
| `FROM_NAME` | Sender name | `Snapdi Team` |
| `ASPNETCORE_ENVIRONMENT` | Environment | `Production` |

### Update Environment Variables

```powershell
# Update a single environment variable
gcloud run services update snapdi-api \
    --region=asia-southeast1 \
    --update-env-vars="APP_BASE_URL=https://your-actual-url.run.app"

# Update multiple variables
gcloud run services update snapdi-api \
    --region=asia-southeast1 \
    --update-env-vars="VAR1=value1,VAR2=value2"
```

---

## 🔧 Manual Deployment

If you prefer manual control over each step:

### 1. Build Locally

```powershell
cd BE2\Snapdi_App_BE\Snapdi
docker build -t snapdi-api:local .
```

### 2. Test Locally

```powershell
# Run container locally
docker run -p 8080:8080 \
    -e CONNECTION_STRING="your-connection-string" \
    -e JWT_KEY="your-jwt-key" \
    snapdi-api:local

# Test API
curl http://localhost:8080/swagger
```

### 3. Tag for Artifact Registry

```powershell
$PROJECT_ID = "your-project-id"
$IMAGE_NAME = "asia-southeast1-docker.pkg.dev/$PROJECT_ID/snapdi-repo/snapdi-api"

docker tag snapdi-api:local ${IMAGE_NAME}:latest
docker tag snapdi-api:local ${IMAGE_NAME}:v1.0.0
```

### 4. Push to Registry

```powershell
docker push ${IMAGE_NAME}:latest
docker push ${IMAGE_NAME}:v1.0.0
```

### 5. Deploy Specific Version

```powershell
gcloud run deploy snapdi-api \
    --image=${IMAGE_NAME}:v1.0.0 \
    --region=asia-southeast1 \
    --platform=managed
```

---

## 📊 Post-Deployment Configuration

### 1. Update APP_BASE_URL

After deployment, update the APP_BASE_URL environment variable:

```powershell
# Get your service URL
$SERVICE_URL = gcloud run services describe snapdi-api --region=asia-southeast1 --format="value(status.url)"

# Update environment variable
gcloud run services update snapdi-api \
    --region=asia-southeast1 \
    --update-env-vars="APP_BASE_URL=$SERVICE_URL"
```

### 2. Test API Endpoints

```powershell
# Test health endpoint
curl https://your-service-url.run.app/api/health

# Access Swagger UI
Start-Process "https://your-service-url.run.app/swagger"

# Test authentication endpoint
curl -X POST https://your-service-url.run.app/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@snapdi.com","password":"Admin@123"}'
```

### 3. Configure Custom Domain (Optional)

1. **Verify domain ownership** in Google Search Console

2. **Map domain to Cloud Run**
   ```powershell
   gcloud run domain-mappings create \
       --service=snapdi-api \
       --domain=api.snapdi.com \
       --region=asia-southeast1
   ```

3. **Update DNS records** as instructed by Google Cloud

### 4. Set Up CORS (if needed)

The application already has CORS configured in `Program.cs`:
```csharp
app.UseCors("AllowFlutterApp");
```

To allow specific domains, update the CORS policy in code before deployment.

---

## 📈 Monitoring and Logs

### View Logs

```powershell
# Real-time logs
gcloud run services logs read snapdi-api \
    --region=asia-southeast1 \
    --follow

# Recent logs
gcloud run services logs read snapdi-api \
    --region=asia-southeast1 \
    --limit=100

# Filter logs
gcloud run services logs read snapdi-api \
    --region=asia-southeast1 \
    --filter="severity>=ERROR"
```

### Cloud Console

Access detailed metrics:
1. Go to: https://console.cloud.google.com/run
2. Click on `snapdi-api` service
3. View tabs: **Metrics**, **Logs**, **Revisions**

### Set Up Alerts

1. Go to **Monitoring** → **Alerting**
2. Create alert for:
   - High error rate (>5%)
   - High latency (>2s)
   - High memory usage (>80%)

---

## 🐛 Troubleshooting

### Common Issues

#### 1. Connection to Supabase Failed

**Error**: `Connection refused` or `SSL connection error`

**Solution**:
- Verify connection string includes `SSL Mode=Require`
- Check Supabase database is running
- Verify password doesn't have special characters that need escaping
- Test connection with psql:
  ```powershell
  psql "postgresql://postgres:$Snapdi$2025@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres?sslmode=require"
  ```

#### 2. JWT Key Too Short

**Error**: `JWT_KEY must be at least 32 characters long`

**Solution**:
```powershell
# Generate secure JWT key
openssl rand -base64 32

# Or use PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

#### 3. Docker Build Failed

**Error**: Build errors during `docker build`

**Solution**:
```powershell
# Clear Docker cache
docker builder prune -a

# Rebuild without cache
docker build --no-cache -t snapdi-api .
```

#### 4. Cloud Run Service Not Responding

**Error**: `502 Bad Gateway` or timeout errors

**Solution**:
- Check service logs for errors
- Verify container port is 8080
- Increase memory allocation:
  ```powershell
  gcloud run services update snapdi-api \
      --region=asia-southeast1 \
      --memory=1Gi
  ```
- Increase timeout:
  ```powershell
  gcloud run services update snapdi-api \
      --region=asia-southeast1 \
      --timeout=600
  ```

#### 5. Environment Variables Not Loading

**Error**: Configuration values are null or default

**Solution**:
- List current environment variables:
  ```powershell
  gcloud run services describe snapdi-api \
      --region=asia-southeast1 \
      --format="value(spec.template.spec.containers[0].env)"
  ```
- Re-apply environment variables
- Check for typos in variable names

### Debug Deployment

```powershell
# Check service status
gcloud run services describe snapdi-api --region=asia-southeast1

# View recent revisions
gcloud run revisions list --service=snapdi-api --region=asia-southeast1

# Rollback to previous revision
gcloud run services update-traffic snapdi-api \
    --region=asia-southeast1 \
    --to-revisions=snapdi-api-00001-abc=100
```

---

## 💰 Cost Optimization

### Cloud Run Pricing (Free Tier)

- **2 million requests/month** - FREE
- **360,000 GB-seconds** - FREE
- **180,000 vCPU-seconds** - FREE

### Optimization Tips

1. **Use min-instances=0** for development
   ```powershell
   gcloud run services update snapdi-api \
       --region=asia-southeast1 \
       --min-instances=0
   ```

2. **Set appropriate memory limits**
   - Start with 512Mi
   - Monitor usage and adjust

3. **Use request-based scaling**
   ```powershell
   gcloud run services update snapdi-api \
       --region=asia-southeast1 \
       --concurrency=80 \
       --cpu-throttling
   ```

4. **Clean up old revisions**
   ```powershell
   # List all revisions
   gcloud run revisions list --service=snapdi-api --region=asia-southeast1
   
   # Delete old revision
   gcloud run revisions delete snapdi-api-00001-abc --region=asia-southeast1
   ```

### Estimated Monthly Cost

With moderate usage (100K requests/month):
- Cloud Run: **FREE** (within free tier)
- Artifact Registry: ~$0.50/month (storage)
- **Total: < $1/month**

---

## 📚 Additional Resources

- [Cloud Run Documentation](https://cloud.google.com/run/docs)
- [Artifact Registry Documentation](https://cloud.google.com/artifact-registry/docs)
- [Supabase Documentation](https://supabase.com/docs)
- [ASP.NET Core on Cloud Run](https://cloud.google.com/dotnet/docs/run)

---

## 🎯 Quick Reference Commands

```powershell
# Deploy
.\deploy-cloudrun.ps1 -ProjectId "your-project-id"

# View logs
gcloud run services logs read snapdi-api --region=asia-southeast1 --follow

# Update env var
gcloud run services update snapdi-api --region=asia-southeast1 --update-env-vars="KEY=value"

# Get service URL
gcloud run services describe snapdi-api --region=asia-southeast1 --format="value(status.url)"

# Delete service
gcloud run services delete snapdi-api --region=asia-southeast1
```

---

**Last Updated**: October 29, 2025

**Deployment Status**: Ready for Production ✅

**Support**: For issues, check logs and troubleshooting section above.

