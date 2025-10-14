#!/bin/bash

# PostgreSQL Migration Helper Script (Linux/Mac)
# Run this script to migrate database from SQL Server to PostgreSQL

echo "=== Snapdi PostgreSQL Migration Helper ==="
echo ""

# Check if .env file exists
if [ ! -f ".env" ]; then
    echo "Creating .env file from .env.example..."
    cp .env.example .env
    echo "Please edit .env file with your PostgreSQL connection string"
    echo "Press any key to continue after editing .env file..."
    read -n 1 -s
fi

# Load environment variables
if [ -f ".env" ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

if [ -z "$CONNECTION_STRING" ]; then
    echo "ERROR: CONNECTION_STRING not found in .env file"
    exit 1
fi

echo "Connection String: $CONNECTION_STRING"
echo ""

# Check if PostgreSQL or SQL Server
if [[ $CONNECTION_STRING == *"Host="* ]]; then
    echo "Detected PostgreSQL connection"
    DB_TYPE="PostgreSQL"
else
    echo "Detected SQL Server connection"
    DB_TYPE="SQL Server"
fi

echo ""
echo "=== Options ==="
echo "1. Create new migration for PostgreSQL"
echo "2. Apply migrations to database"
echo "3. Remove last migration"
echo "4. List all migrations"
echo "5. Drop database (WARNING: All data will be lost)"
echo "6. Exit"
echo ""

read -p "Select option (1-6): " choice

case $choice in
    1)
        read -p "Enter migration name (e.g., InitialPostgreSQL): " migration_name
        echo "Creating migration: $migration_name"
        
        cd Snapdi.Api
        dotnet ef migrations add $migration_name --project ../Snapdi.Repositories --verbose
        cd ..
        
        echo "Migration created successfully!"
        ;;
    
    2)
        echo "Applying migrations to database..."
        
        cd Snapdi.Api
        dotnet ef database update --project ../Snapdi.Repositories --verbose
        cd ..
        
        echo "Migrations applied successfully!"
        ;;
    
    3)
        echo "Removing last migration..."
        
        cd Snapdi.Api
        dotnet ef migrations remove --project ../Snapdi.Repositories --force
        cd ..
        
        echo "Migration removed successfully!"
        ;;
    
    4)
        echo "Listing all migrations..."
        
        cd Snapdi.Api
        dotnet ef migrations list --project ../Snapdi.Repositories
        cd ..
        ;;
    
    5)
        echo "WARNING: This will drop the entire database!"
        read -p "Are you sure? Type 'YES' to confirm: " confirm
        
        if [ "$confirm" = "YES" ]; then
            echo "Dropping database..."
            
            cd Snapdi.Api
            dotnet ef database drop --project ../Snapdi.Repositories --force
            cd ..
            
            echo "Database dropped successfully!"
        else
            echo "Operation cancelled"
        fi
        ;;
    
    6)
        echo "Goodbye!"
        exit 0
        ;;
    
    *)
        echo "Invalid option"
        ;;
esac

echo ""
echo "Done!"
