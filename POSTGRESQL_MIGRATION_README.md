# PostgreSQL Migration - Snapdi Backend

## ✅ Migration Completed Successfully

Your ASP.NET Backend has been successfully migrated from SQL Server to PostgreSQL.

## Changes Made

### 1. Removed SQL Server Dependencies
- **File Modified**: `Snapdi.Repositories/Snapdi.Repositories.csproj`
- **Change**: Removed `Microsoft.EntityFrameworkCore.SqlServer` package reference
- **Result**: Your project now depends only on PostgreSQL (Npgsql) for database operations

### 2. Updated DbContext Configuration
- **File Modified**: `Snapdi.Repositories/Context/SnapdiDbV2Context.cs`
- **Change**: Updated `OnConfiguring` method from `UseSqlServer` to `UseNpgsql`
- **Result**: Entity Framework Core now uses PostgreSQL provider for spatial data (NetTopologySuite)

### 3. Generated Complete PostgreSQL Database Script
- **New File**: `Scripts/init-database-postgres.sql`
- **Contents**: 
  - PostGIS extension setup for spatial data support
  - Complete database schema (19 tables)
  - All relationships, foreign keys, and constraints
  - Proper PostgreSQL data types
  - Unique indexes

### 4. Added Seed Data
The initialization script includes default data:

**Roles:**
- RoleID: 1 → `ADMIN`
- RoleID: 2 → `CUSTOMER`
- RoleID: 3 → `PHOTOGRAPHER`

**BookingStatus:**
- StatusID: 1-7 → Pending, Confirmed, Paid, Going, Processing, Done, Completed

**PaymentStatus:**
- PaymentStatusID: 1-4 → Pending, Paid, Refunded, Confirmed

**Default Admin User:**
- Email: `admin@snapdi.com`
- Password: `$2y$10$zW75IVkYdDjmMPRZBDz9r.2/iXlAlby5NWm6dq6EpiuGFlcacVPDS`
- Role: ADMIN
- IsActive: true
- IsVerify: true

## How to Use the Database Script

### Option 1: Fresh Database Setup

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE snapdi_db;

# Connect to the database
\c snapdi_db

# Run the initialization script
\i BE2/Snapdi_App_BE/Snapdi/Scripts/init-database-postgres.sql
```

### Option 2: Using psql command line

```bash
psql -U postgres -d snapdi_db -f BE2/Snapdi_App_BE/Snapdi/Scripts/init-database-postgres.sql
```

### Option 3: Using pgAdmin or other GUI tools

1. Open pgAdmin or your preferred PostgreSQL GUI tool
2. Connect to your PostgreSQL server
3. Create a new database named `snapdi_db`
4. Open the Query Tool
5. Open and execute the `init-database-postgres.sql` script

## Database Connection String

Your current connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123;Include Error Detail=true"
  }
}
```

Update this with your actual PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=snapdi_db;Username=your_username;Password=your_password;Include Error Detail=true"
  }
}
```

## PostgreSQL Data Type Mappings

The migration converted SQL Server types to PostgreSQL equivalents:

| SQL Server | PostgreSQL | Usage |
|------------|------------|-------|
| `int IDENTITY` | `SERIAL` | Auto-increment primary keys |
| `nvarchar(n)` | `VARCHAR(n)` | Variable-length strings |
| `nvarchar(MAX)` | `TEXT` | Unlimited text |
| `datetime` | `TIMESTAMP` | Date and time |
| `bit` | `BOOLEAN` | True/false values |
| `float` | `DOUBLE PRECISION` | Decimal numbers |
| `geography` | `geography(Point, 4326)` | Spatial data (PostGIS) |

## PostGIS Spatial Data Support

The database includes PostGIS support for the `CurrentLocation` field in the `User` table:

```sql
"CurrentLocation" geography(Point, 4326)
```

This stores latitude/longitude coordinates in WGS84 coordinate system (SRID 4326).

**Usage Example in C#:**
```csharp
using NetTopologySuite.Geometries;

// Create a point (longitude, latitude)
var point = new Point(-122.419, 37.775) { SRID = 4326 };
user.CurrentLocation = point;
```

## Verification Steps

After running the script and starting your API:

1. **Check Database Connection:**
   ```bash
   dotnet run --project BE2/Snapdi_App_BE/Snapdi/Snapdi.Api
   ```

2. **Test API Endpoints:**
   - Navigate to `https://localhost:7000/swagger`
   - Try the authentication endpoints with the default admin user

3. **Verify Seed Data:**
   ```sql
   SELECT * FROM "Role";
   SELECT * FROM "BookingStatus";
   SELECT * FROM "PaymentStatus";
   SELECT * FROM "User" WHERE "Email" = 'admin@snapdi.com';
   ```

## Troubleshooting

### PostGIS Extension Error
If you get an error about PostGIS:
```bash
# Install PostGIS extension
# On Ubuntu/Debian:
sudo apt-get install postgresql-<version>-postgis-3

# On macOS with Homebrew:
brew install postgis
```

### Connection Issues
- Ensure PostgreSQL is running: `sudo systemctl status postgresql`
- Check firewall settings allow connections to port 5432
- Verify credentials in connection string

### Migration History
If you need to update EF Core migrations:
```bash
cd BE2/Snapdi_App_BE/Snapdi/Snapdi.Repositories
dotnet ef migrations add MigrationName --startup-project ../Snapdi.Api
dotnet ef database update --startup-project ../Snapdi.Api
```

## Notes

- The script drops existing tables before creating new ones - **use with caution on existing databases**
- All foreign keys include proper CASCADE behaviors for related deletions
- Sequences are properly set after seed data insertion to avoid ID conflicts
- The database uses quoted identifiers to preserve case-sensitivity (PostgreSQL default is lowercase)

## Next Steps

1. ✅ Run the initialization script on your PostgreSQL server
2. ✅ Update the connection string in `appsettings.json`
3. ✅ Test the API with Swagger
4. ✅ Verify the default admin login works
5. ✅ Deploy to your target environment (Render, AWS, etc.)

## Support

If you encounter any issues:
1. Check PostgreSQL logs: `/var/log/postgresql/`
2. Review application logs in the console
3. Verify all packages are restored: `dotnet restore`
4. Ensure .NET 8.0 SDK is installed

---

**Migration completed on:** October 28, 2025
**Target Database:** PostgreSQL 12+
**EF Core Version:** 8.0.20
**Npgsql Version:** 8.0.11

