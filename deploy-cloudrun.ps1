# =============================================
# Snapdi API - Google Cloud Run Deployment Script
# =============================================
# This script automates the deployment of Snapdi API to Google Cloud Run
# Prerequisites:
# - Google Cloud SDK installed and authenticated
# - Docker installed
# - .env file configured in Snapdi.Api directory
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$ProjectId = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Region = "asia-southeast1",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "snapdi-api",
    
    [Parameter(Mandatory=$false)]
    [string]$Memory = "512Mi",
    
    [Parameter(Mandatory=$false)]
    [int]$MaxInstances = 10
)

# Color output functions
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Success { Write-ColorOutput Green @args }
function Write-Info { Write-ColorOutput Cyan @args }
function Write-Warning { Write-ColorOutput Yellow @args }
function Write-Error { Write-ColorOutput Red @args }

Write-Info "============================================="
Write-Info "   Snapdi API - Cloud Run Deployment"
Write-Info "============================================="
Write-Host ""

# Check if gcloud is installed
try {
    $gcloudVersion = gcloud version 2>&1 | Select-String "Google Cloud SDK"
    Write-Success "✓ Google Cloud SDK is installed"
} catch {
    Write-Error "✗ Google Cloud SDK is not installed!"
    Write-Info "Please install from: https://cloud.google.com/sdk/docs/install"
    exit 1
}

# Get or validate Project ID
if ([string]::IsNullOrEmpty($ProjectId)) {
    $currentProject = gcloud config get-value project 2>$null
    if ([string]::IsNullOrEmpty($currentProject)) {
        Write-Warning "No project ID specified and no default project set."
        $ProjectId = Read-Host "Enter your GCP Project ID"
    } else {
        $ProjectId = $currentProject
        Write-Info "Using current project: $ProjectId"
    }
}

Write-Info "Project ID: $ProjectId"
Write-Info "Region: $Region"
Write-Info "Service Name: $ServiceName"
Write-Host ""

# Set the project
Write-Info "Setting GCP project..."
gcloud config set project $ProjectId
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to set project. Please check your project ID."
    exit 1
}
Write-Success "✓ Project set successfully"
Write-Host ""

# Enable required APIs
Write-Info "Enabling required Google Cloud APIs..."
$apis = @(
    "run.googleapis.com",
    "artifactregistry.googleapis.com",
    "cloudbuild.googleapis.com"
)

foreach ($api in $apis) {
    Write-Info "  Enabling $api..."
    gcloud services enable $api --quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Success "  ✓ $api enabled"
    }
}
Write-Host ""

# Create Artifact Registry repository
$repoName = "snapdi-repo"
Write-Info "Creating Artifact Registry repository..."
gcloud artifacts repositories describe $repoName --location=$Region 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Info "  Creating new repository: $repoName"
    gcloud artifacts repositories create $repoName `
        --repository-format=docker `
        --location=$Region `
        --description="Snapdi API Docker images" `
        --quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Success "✓ Repository created successfully"
    }
} else {
    Write-Success "✓ Repository already exists"
}
Write-Host ""

# Build image path
$imageName = "$Region-docker.pkg.dev/$ProjectId/$repoName/$ServiceName"
$imageTag = "latest"
$fullImagePath = "${imageName}:${imageTag}"

Write-Info "Image path: $fullImagePath"
Write-Host ""

# Change to Snapdi directory
$snapdiPath = Join-Path $PSScriptRoot "Snapdi_App_BE\Snapdi"
if (-not (Test-Path $snapdiPath)) {
    Write-Error "Snapdi directory not found at: $snapdiPath"
    exit 1
}

Set-Location $snapdiPath
Write-Success "✓ Changed to Snapdi directory"
Write-Host ""

# Build Docker image
Write-Info "Building Docker image..."
Write-Info "This may take several minutes..."
docker build -t $fullImagePath .
if ($LASTEXITCODE -ne 0) {
    Write-Error "✗ Docker build failed!"
    exit 1
}
Write-Success "✓ Docker image built successfully"
Write-Host ""

# Configure Docker for Artifact Registry
Write-Info "Configuring Docker authentication for Artifact Registry..."
gcloud auth configure-docker "$Region-docker.pkg.dev" --quiet
Write-Success "✓ Docker authentication configured"
Write-Host ""

# Push image to Artifact Registry
Write-Info "Pushing Docker image to Artifact Registry..."
Write-Info "This may take several minutes..."
docker push $fullImagePath
if ($LASTEXITCODE -ne 0) {
    Write-Error "✗ Failed to push Docker image!"
    exit 1
}
Write-Success "✓ Docker image pushed successfully"
Write-Host ""

# Load environment variables from .env file
$envFile = Join-Path $snapdiPath "Snapdi.Api\.env"
$envVars = @()

if (Test-Path $envFile) {
    Write-Info "Loading environment variables from .env file..."
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line -and !$line.StartsWith("#")) {
            $parts = $line -split "=", 2
            if ($parts.Length -eq 2) {
                $key = $parts[0].Trim()
                $value = $parts[1].Trim()
                $envVars += "$key=$value"
            }
        }
    }
    Write-Success "✓ Loaded $($envVars.Count) environment variables"
} else {
    Write-Warning "No .env file found at: $envFile"
    Write-Info "Environment variables will need to be set manually."
}
Write-Host ""

# Deploy to Cloud Run
Write-Info "Deploying to Cloud Run..."
Write-Info "Service: $ServiceName"
Write-Info "Region: $Region"
Write-Info "Memory: $Memory"
Write-Info "Max Instances: $MaxInstances"
Write-Host ""

$deployArgs = @(
    "run", "deploy", $ServiceName,
    "--image=$fullImagePath",
    "--platform=managed",
    "--region=$Region",
    "--allow-unauthenticated",
    "--port=8080",
    "--memory=$Memory",
    "--cpu=1",
    "--min-instances=0",
    "--max-instances=$MaxInstances",
    "--timeout=300",
    "--quiet"
)

# Add environment variables
if ($envVars.Count -gt 0) {
    $envString = $envVars -join ","
    $deployArgs += "--set-env-vars=$envString"
}

gcloud @deployArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "✗ Cloud Run deployment failed!"
    exit 1
}

Write-Host ""
Write-Success "============================================="
Write-Success "   Deployment Completed Successfully! "
Write-Success "============================================="
Write-Host ""

# Get service URL
$serviceUrl = gcloud run services describe $ServiceName --region=$Region --format="value(status.url)" 2>$null
if ($serviceUrl) {
    Write-Success "Service URL: $serviceUrl"
    Write-Info "Swagger UI: $serviceUrl/swagger"
    Write-Host ""
    Write-Info "Test your API with:"
    Write-Info "  curl $serviceUrl/api/health"
    Write-Host ""
}

Write-Info "Next steps:"
Write-Info "1. Update APP_BASE_URL environment variable with: $serviceUrl"
Write-Info "2. Test the API endpoints at: $serviceUrl/swagger"
Write-Info "3. Configure custom domain (optional)"
Write-Info "4. Set up monitoring and alerts"
Write-Host ""

Write-Success "Deployment complete! 🎉"

