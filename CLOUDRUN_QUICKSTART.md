# Cloud Run Quick Start Guide

Quick reference for deploying Snapdi API to Google Cloud Run.

## 🚀 One-Command Deployment

```powershell
cd BE2\Snapdi_App_BE
.\deploy-cloudrun.ps1 -ProjectId "your-gcp-project-id" -Region "asia-southeast1"
```

---

## 📝 Manual Deployment Commands

### 1. Initial Setup

```powershell
# Set your project ID
$PROJECT_ID = "your-gcp-project-id"
$REGION = "asia-southeast1"
$SERVICE_NAME = "snapdi-api"

# Set active project
gcloud config set project $PROJECT_ID

# Enable required APIs
gcloud services enable run.googleapis.com
gcloud services enable artifactregistry.googleapis.com
gcloud services enable cloudbuild.googleapis.com
```

### 2. Create Artifact Registry Repository

```powershell
# Create Docker repository
gcloud artifacts repositories create snapdi-repo `
    --repository-format=docker `
    --location=$REGION `
    --description="Snapdi API Docker images"

# Configure Docker authentication
gcloud auth configure-docker "$REGION-docker.pkg.dev"
```

### 3. Build and Push Docker Image

```powershell
# Navigate to Snapdi directory
cd BE2\Snapdi_App_BE\Snapdi

# Build Docker image
$IMAGE_NAME = "$REGION-docker.pkg.dev/$PROJECT_ID/snapdi-repo/$SERVICE_NAME"
docker build -t "${IMAGE_NAME}:latest" .

# Push to Artifact Registry
docker push "${IMAGE_NAME}:latest"
```

### 4. Deploy to Cloud Run

```powershell
# Deploy with environment variables
gcloud run deploy $SERVICE_NAME `
    --image="${IMAGE_NAME}:latest" `
    --platform=managed `
    --region=$REGION `
    --allow-unauthenticated `
    --port=8080 `
    --memory=512Mi `
    --cpu=1 `
    --min-instances=0 `
    --max-instances=10 `
    --timeout=300 `
    --set-env-vars="CONNECTION_STRING=Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=`$Snapdi`$2025;SSL Mode=Require;Trust Server Certificate=true,JWT_KEY=your-secure-jwt-key-minimum-32-characters-long,JWT_ISSUER=SnapdiAPI,JWT_AUDIENCE=SnapdiClient,JWT_EXPIRATION_HOURS=24,ASPNETCORE_ENVIRONMENT=Production,CLOUDINARY_CLOUD_NAME=your_cloud_name,CLOUDINARY_API_KEY=your_api_key,CLOUDINARY_API_SECRET=your_api_secret"
```

### 5. Get Service URL

```powershell
# Get deployed service URL
$SERVICE_URL = gcloud run services describe $SERVICE_NAME `
    --region=$REGION `
    --format="value(status.url)"

Write-Host "Service URL: $SERVICE_URL"
Write-Host "Swagger UI: $SERVICE_URL/swagger"
```

### 6. Update APP_BASE_URL

```powershell
# Update APP_BASE_URL with actual service URL
gcloud run services update $SERVICE_NAME `
    --region=$REGION `
    --update-env-vars="APP_BASE_URL=$SERVICE_URL"
```

---

## 🔄 Update Deployment

```powershell
# Rebuild and redeploy
cd BE2\Snapdi_App_BE\Snapdi
docker build -t "${IMAGE_NAME}:latest" .
docker push "${IMAGE_NAME}:latest"

gcloud run deploy $SERVICE_NAME `
    --image="${IMAGE_NAME}:latest" `
    --region=$REGION `
    --platform=managed
```

---

## 🔐 Environment Variables from .env File

If you have a `.env` file in `Snapdi\Snapdi.Api\.env`:

```powershell
# Load from .env and deploy
$envFile = "Snapdi\Snapdi.Api\.env"
$envVars = @()

Get-Content $envFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -and !$line.StartsWith("#")) {
        $parts = $line -split "=", 2
        if ($parts.Length -eq 2) {
            $envVars += "$($parts[0].Trim())=$($parts[1].Trim())"
        }
    }
}

$envString = $envVars -join ","

gcloud run deploy $SERVICE_NAME `
    --image="${IMAGE_NAME}:latest" `
    --region=$REGION `
    --platform=managed `
    --set-env-vars="$envString"
```

---

## 📊 Useful Commands

```powershell
# View logs
gcloud run services logs read $SERVICE_NAME --region=$REGION --follow

# Describe service
gcloud run services describe $SERVICE_NAME --region=$REGION

# List revisions
gcloud run revisions list --service=$SERVICE_NAME --region=$REGION

# Update environment variable
gcloud run services update $SERVICE_NAME `
    --region=$REGION `
    --update-env-vars="KEY=value"

# Update memory
gcloud run services update $SERVICE_NAME `
    --region=$REGION `
    --memory=1Gi

# Delete service
gcloud run services delete $SERVICE_NAME --region=$REGION
```

---

## 🧪 Testing

```powershell
# Test API
curl "$SERVICE_URL/api/health"

# Open Swagger UI
Start-Process "$SERVICE_URL/swagger"

# Test authentication
curl -X POST "$SERVICE_URL/api/auth/login" `
    -H "Content-Type: application/json" `
    -d '{\"email\":\"admin@snapdi.com\",\"password\":\"Admin@123\"}'
```

---

## 🔑 Generate JWT Key

```powershell
# Using OpenSSL (if installed)
openssl rand -base64 32

# Using PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

---

## 📍 Regions

Popular regions for Southeast Asia:

- `asia-southeast1` - Singapore (recommended)
- `asia-southeast2` - Jakarta
- `asia-east1` - Taiwan
- `asia-northeast1` - Tokyo

---

## 💡 Tips

1. **Use automated script for easier deployment**: `.\deploy-cloudrun.ps1`
2. **Store secrets in Secret Manager** for production
3. **Monitor costs** in Cloud Console
4. **Set up alerts** for errors and high latency
5. **Use min-instances=0** for dev/test to save costs
6. **Use min-instances=1** for production to avoid cold starts

---

For detailed information, see [GOOGLE_CLOUD_RUN_DEPLOYMENT.md](GOOGLE_CLOUD_RUN_DEPLOYMENT.md)

