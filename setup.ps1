# Snapdi Backend API - Quick Setup Script
# Chạy script này để setup project lần đầu

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Snapdi Backend API - Quick Setup    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host ""

# Check Docker (optional)
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker installed: $dockerVersion" -ForegroundColor Green
    $hasDocker = $true
} catch {
    Write-Host "⚠ Docker not found (optional for PostgreSQL)" -ForegroundColor Yellow
    $hasDocker = $false
}

Write-Host ""

# Navigate to project directory
Set-Location "Snapdi"

# Check .env file
if (-not (Test-Path ".env")) {
    Write-Host "Creating .env file from template..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "✓ .env file created" -ForegroundColor Green
    Write-Host ""
    Write-Host "⚠ IMPORTANT: Please edit .env file with your configuration!" -ForegroundColor Yellow
    Write-Host "  Required settings:" -ForegroundColor Yellow
    Write-Host "  - CONNECTION_STRING (your database connection)" -ForegroundColor Yellow
    Write-Host "  - JWT_KEY (minimum 32 characters)" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "✓ .env file already exists" -ForegroundColor Green
}

# Restore NuGet packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Choose Development Environment      " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. SQL Server (Local Development)" -ForegroundColor White
Write-Host "   - Use existing SQL Server instance" -ForegroundColor Gray
Write-Host "   - Connection: localhost" -ForegroundColor Gray
Write-Host ""
Write-Host "2. PostgreSQL with Docker (Recommended)" -ForegroundColor White
Write-Host "   - Run PostgreSQL in Docker container" -ForegroundColor Gray
Write-Host "   - Easy setup and cleanup" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Skip database setup" -ForegroundColor White
Write-Host "   - Setup database manually later" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Select option (1-3)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "Using SQL Server..." -ForegroundColor Yellow
        Write-Host "Make sure your SQL Server is running and .env has correct connection string" -ForegroundColor White
        Write-Host ""
        
        $runMigration = Read-Host "Do you want to run database migrations now? (y/n)"
        if ($runMigration -eq "y" -or $runMigration -eq "Y") {
            Write-Host "Running migrations..." -ForegroundColor Yellow
            Set-Location "Snapdi.Api"
            dotnet ef database update --project ../Snapdi.Repositories
            Set-Location ".."
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Database migrated successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ Migration failed. Please check connection string and try again" -ForegroundColor Red
            }
        }
    }
    
    "2" {
        if (-not $hasDocker) {
            Write-Host ""
            Write-Host "✗ Docker is required for this option" -ForegroundColor Red
            Write-Host "  Please install Docker Desktop and try again" -ForegroundColor Yellow
            exit 1
        }
        
        Write-Host ""
        Write-Host "Starting PostgreSQL with Docker..." -ForegroundColor Yellow
        docker-compose up -d postgres
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ PostgreSQL container started" -ForegroundColor Green
            Write-Host ""
            Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
            
            Write-Host "Updating .env with PostgreSQL connection..." -ForegroundColor Yellow
            $pgConnString = "Host=localhost;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123"
            
            # Update .env file
            $envContent = Get-Content ".env"
            $envContent = $envContent -replace '^CONNECTION_STRING=.*', "CONNECTION_STRING=$pgConnString"
            $envContent | Set-Content ".env"
            
            Write-Host "✓ .env updated with PostgreSQL connection" -ForegroundColor Green
            Write-Host ""
            
            $runMigration = Read-Host "Do you want to run database migrations now? (y/n)"
            if ($runMigration -eq "y" -or $runMigration -eq "Y") {
                Write-Host "Running migrations..." -ForegroundColor Yellow
                Set-Location "Snapdi.Api"
                dotnet ef database update --project ../Snapdi.Repositories
                Set-Location ".."
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✓ Database migrated successfully" -ForegroundColor Green
                } else {
                    Write-Host "✗ Migration failed" -ForegroundColor Red
                }
            }
        } else {
            Write-Host "✗ Failed to start PostgreSQL container" -ForegroundColor Red
        }
    }
    
    "3" {
        Write-Host ""
        Write-Host "Skipping database setup" -ForegroundColor Yellow
        Write-Host "You can setup database later using:" -ForegroundColor White
        Write-Host "  - .\migrate.ps1 (migration helper)" -ForegroundColor Gray
        Write-Host "  - docker-compose up (full stack)" -ForegroundColor Gray
    }
    
    default {
        Write-Host ""
        Write-Host "Invalid option" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "         Setup Complete! 🎉            " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Review and edit .env file if needed" -ForegroundColor White
Write-Host "   - Set JWT_KEY (minimum 32 characters)" -ForegroundColor Gray
Write-Host "   - Configure email settings (optional)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor White
Write-Host "   cd Snapdi.Api" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Or use Docker Compose (full stack):" -ForegroundColor White
Write-Host "   docker-compose up --build" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Access API:" -ForegroundColor White
Write-Host "   Swagger UI: http://localhost:8080/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "📚 Documentation:" -ForegroundColor Yellow
Write-Host "   - DOCKER_GUIDE.md - Docker usage guide" -ForegroundColor Gray
Write-Host "   - RENDER_DEPLOYMENT_GUIDE.md - Deploy to Render" -ForegroundColor Gray
Write-Host "   - README.md - Full documentation" -ForegroundColor Gray
Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green
Write-Host ""
