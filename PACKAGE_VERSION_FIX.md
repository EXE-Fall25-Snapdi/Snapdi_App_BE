# 🔧 Quick Fix: PostgreSQL Package Version Conflict

## ⚠️ Vấn đề

```
System.TypeLoadException: Method 'get_LockReleaseBehavior' in type 
'Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal.SqlServerHistoryRepository' 
from assembly 'Microsoft.EntityFrameworkCore.SqlServer, Version=8.0.20.0' 
does not have an implementation.
```

**Nguyên nhân**: Version conflict giữa packages
- `Npgsql.EntityFrameworkCore.PostgreSQL` version **9.0.4** (API project)
- `Npgsql.EntityFrameworkCore.PostgreSQL` version **8.0.11** (Repositories project)  
- `Microsoft.EntityFrameworkCore.SqlServer` version **8.0.20**

## ✅ Giải pháp đã áp dụng

Downgrade `Npgsql.EntityFrameworkCore.PostgreSQL` trong API project về **8.0.11** để match với Repositories project.

### File đã sửa:
`Snapdi.Api/Snapdi.Api.csproj`

```xml
<!-- BEFORE -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />

<!-- AFTER -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
```

## 🔄 Bước tiếp theo

### 1. Restore packages

```powershell
cd C:\Users\enteecaay\Desktop\Snapdi\BE2\Snapdi_App_BE\Snapdi
dotnet restore
```

### 2. Clean và rebuild

```powershell
dotnet clean
dotnet build
```

### 3. Chạy lại API

```powershell
cd Snapdi.Api
dotnet run
```

## 📊 Package Versions Summary

Sau khi fix, tất cả packages sẽ dùng version 8.0.x:

| Package | Version |
|---------|---------|
| Microsoft.EntityFrameworkCore | 8.0.20 |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.20 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 |
| Microsoft.EntityFrameworkCore.Design | 8.0.20 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.20 |
| dotnet-ef (global tool) | 8.0.11 |

## ⚡ Alternative: Nếu vẫn lỗi

### Option 1: Xóa bin/obj folders

```powershell
cd C:\Users\enteecaay\Desktop\Snapdi\BE2\Snapdi_App_BE\Snapdi
Remove-Item -Recurse -Force */bin,*/obj
dotnet restore
dotnet build
```

### Option 2: Clear NuGet cache

```powershell
dotnet nuget locals all --clear
dotnet restore
```

### Option 3: Dùng EnsureCreated thay vì Migrations

Thêm vào `Program.cs`:

```csharp
var app = builder.Build();

// For development only - create database if not exists
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SnapdiDbV2Context>();
        dbContext.Database.EnsureCreated(); // Creates DB without migrations
    }
}
```

**Note**: `EnsureCreated()` sẽ tạo database từ model hiện tại, không dùng migrations.

## 🎯 Expected Result

Sau khi fix và restart, bạn sẽ thấy:

```
✅ Database migrated successfully!
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5148
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## 🔍 Verify Fix

Test API endpoints:

```bash
# Swagger
http://localhost:5148/swagger

# Health check
http://localhost:5148/api/health/detailed
```

Response từ health check sẽ show:

```json
{
  "checks": {
    "database": {
      "status": "healthy",
      "provider": "PostgreSQL",
      "connected": true
    }
  }
}
```

## 📝 Commit Changes

Sau khi verify thành công:

```powershell
git add .
git commit -m "fix: Downgrade Npgsql package to 8.0.11 for version compatibility"
git push origin test-deploy
```

---

**Status**: ✅ Fix applied, waiting for rebuild & test
