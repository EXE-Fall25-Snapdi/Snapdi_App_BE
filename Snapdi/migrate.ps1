# PostgreSQL Migration Helper Script
# Run this script to migrate database from SQL Server to PostgreSQL

Write-Host "=== Snapdi PostgreSQL Migration Helper ===" -ForegroundColor Cyan
Write-Host ""

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "Creating .env file from .env.example..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env"
    Write-Host "Please edit .env file with your PostgreSQL connection string" -ForegroundColor Red
    Write-Host "Press any key to continue after editing .env file..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# Load environment variables
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($key, $value, "Process")
        }
    }
}

$connectionString = $env:CONNECTION_STRING

if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "ERROR: CONNECTION_STRING not found in .env file" -ForegroundColor Red
    exit 1
}

Write-Host "Connection String: $connectionString" -ForegroundColor Green
Write-Host ""

# Check if PostgreSQL or SQL Server
if ($connectionString -like "*Host=*") {
    Write-Host "Detected PostgreSQL connection" -ForegroundColor Green
    $dbType = "PostgreSQL"
} else {
    Write-Host "Detected SQL Server connection" -ForegroundColor Green
    $dbType = "SQL Server"
}

Write-Host ""
Write-Host "=== Options ===" -ForegroundColor Cyan
Write-Host "1. Create new migration for PostgreSQL"
Write-Host "2. Apply migrations to database"
Write-Host "3. Remove last migration"
Write-Host "4. List all migrations"
Write-Host "5. Drop database (WARNING: All data will be lost)"
Write-Host "6. Exit"
Write-Host ""

$choice = Read-Host "Select option (1-6)"

switch ($choice) {
    "1" {
        $migrationName = Read-Host "Enter migration name (e.g., InitialPostgreSQL)"
        Write-Host "Creating migration: $migrationName" -ForegroundColor Yellow
        
        Set-Location "Snapdi.Api"
        dotnet ef migrations add $migrationName --project ../Snapdi.Repositories --verbose
        Set-Location ".."
        
        Write-Host "Migration created successfully!" -ForegroundColor Green
    }
    
    "2" {
        Write-Host "Applying migrations to database..." -ForegroundColor Yellow
        
        Set-Location "Snapdi.Api"
        
        if ($dbType -eq "PostgreSQL") {
            dotnet ef database update --project ../Snapdi.Repositories --verbose
        } else {
            dotnet ef database update --project ../Snapdi.Repositories --verbose
        }
        
        Set-Location ".."
        
        Write-Host "Migrations applied successfully!" -ForegroundColor Green
    }
    
    "3" {
        Write-Host "Removing last migration..." -ForegroundColor Yellow
        
        Set-Location "Snapdi.Api"
        dotnet ef migrations remove --project ../Snapdi.Repositories --force
        Set-Location ".."
        
        Write-Host "Migration removed successfully!" -ForegroundColor Green
    }
    
    "4" {
        Write-Host "Listing all migrations..." -ForegroundColor Yellow
        
        Set-Location "Snapdi.Api"
        dotnet ef migrations list --project ../Snapdi.Repositories
        Set-Location ".."
    }
    
    "5" {
        Write-Host "WARNING: This will drop the entire database!" -ForegroundColor Red
        $confirm = Read-Host "Are you sure? Type 'YES' to confirm"
        
        if ($confirm -eq "YES") {
            Write-Host "Dropping database..." -ForegroundColor Yellow
            
            Set-Location "Snapdi.Api"
            dotnet ef database drop --project ../Snapdi.Repositories --force
            Set-Location ".."
            
            Write-Host "Database dropped successfully!" -ForegroundColor Green
        } else {
            Write-Host "Operation cancelled" -ForegroundColor Yellow
        }
    }
    
    "6" {
        Write-Host "Goodbye!" -ForegroundColor Cyan
        exit 0
    }
    
    default {
        Write-Host "Invalid option" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
