# Hướng dẫn Deploy Snapdi Backend API lên Render

## 📋 Yêu cầu trước khi deploy

1. Tài khoản Render (https://render.com)
2. Repository GitHub đã push code lên
3. PostgreSQL database trên Render

## 🗄️ Bước 1: Tạo PostgreSQL Database trên Render

1. Đăng nhập vào Render Dashboard
2. Click **"New +"** → Chọn **"PostgreSQL"**
3. Điền thông tin:
   - **Name**: `snapdi-database` (hoặc tên bạn muốn)
   - **Database**: `snapdi_db`
   - **User**: `snapdi_user`
   - **Region**: Chọn Singapore hoặc gần nhất với người dùng
   - **Plan**: Free (hoặc plan phù hợp)
4. Click **"Create Database"**
5. Sau khi tạo xong, copy **Internal Database URL** (dạng: `postgresql://user:password@host:5432/database`)

## 🚀 Bước 2: Deploy Web Service (API)

1. Từ Render Dashboard, click **"New +"** → Chọn **"Web Service"**
2. Connect repository GitHub của bạn
3. Chọn repository `Snapdi_App_BE`
4. Điền thông tin:
   
   ### Basic Settings:
   - **Name**: `snapdi-api`
   - **Region**: Chọn cùng region với database
   - **Branch**: `kietnt` (hoặc branch bạn muốn)
   - **Root Directory**: `Snapdi_App_BE/Snapdi`
   - **Environment**: `Docker`
   - **Dockerfile Path**: `./Dockerfile`
   
   ### Advanced Settings:
   - **Docker Command**: Để trống (sử dụng ENTRYPOINT từ Dockerfile)
   - **Auto-Deploy**: Yes

5. Thêm **Environment Variables**:

   ```
   CONNECTION_STRING = <Paste Internal Database URL từ bước 1>
   
   JWT_KEY = your-super-secret-jwt-key-minimum-32-characters-long-for-security
   
   JWT_ISSUER = SnapdiAPI
   
   JWT_AUDIENCE = SnapdiClient
   
   JWT_EXPIRATION_HOURS = 24
   
   APP_BASE_URL = https://snapdi-api.onrender.com
   
   ASPNETCORE_ENVIRONMENT = Production
   
   # Email settings (nếu có)
   SMTP_HOST = smtp.gmail.com
   SMTP_PORT = 587
   SMTP_USERNAME = your-email@gmail.com
   SMTP_PASSWORD = your-app-password
   FROM_EMAIL = your-email@gmail.com
   FROM_NAME = Snapdi Team
   ```

6. Click **"Create Web Service"**

## 📊 Bước 3: Migrate Database

Sau khi service deploy thành công, bạn cần migrate database schema từ SQL Server sang PostgreSQL.

### Option 1: Sử dụng EF Core Migrations (Khuyên dùng)

1. Trên local, update connection string trong `.env` sang PostgreSQL
2. Chạy lệnh tạo migration mới:
   ```bash
   cd BE2/Snapdi_App_BE/Snapdi/Snapdi.Api
   dotnet ef migrations add InitialPostgreSQL --project ../Snapdi.Repositories
   ```

3. Update database trên Render:
   ```bash
   # Thay <CONNECTION_STRING> bằng External Database URL từ Render
   dotnet ef database update --project ../Snapdi.Repositories --connection "<CONNECTION_STRING>"
   ```

### Option 2: Export/Import Database

1. Export schema từ SQL Server:
   - Sử dụng tools như `pg_dump` hoặc export SQL script
   
2. Convert SQL Server syntax sang PostgreSQL syntax:
   - Thay đổi data types (e.g., `NVARCHAR` → `VARCHAR`, `DATETIME` → `TIMESTAMP`)
   - Thay đổi identity columns: `IDENTITY(1,1)` → `SERIAL` hoặc `GENERATED ALWAYS AS IDENTITY`
   - Update functions và stored procedures syntax
   
3. Import vào PostgreSQL trên Render:
   - Connect vào database qua External Database URL
   - Run SQL script

## 🔍 Bước 4: Kiểm tra Deployment

1. Mở Logs tab trên Render Dashboard
2. Đợi build và deploy hoàn tất (khoảng 5-10 phút)
3. Kiểm tra API endpoint:
   ```
   https://snapdi-api.onrender.com/swagger
   ```

## ⚙️ Test Local với Docker

Trước khi deploy lên Render, bạn có thể test local:

```bash
# Di chuyển vào thư mục chứa docker-compose.yml
cd BE2/Snapdi_App_BE/Snapdi

# Build và chạy containers
docker-compose up --build

# API sẽ chạy tại: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

## 📝 Lưu ý quan trọng

### 1. SSL Mode cho PostgreSQL
Connection string phải có `SSL Mode=Require` khi connect tới Render PostgreSQL:
```
Host=xxx.oregon-postgres.render.com;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=xxx;SSL Mode=Require;Trust Server Certificate=true
```

### 2. Free Plan Limitations
- Database: 1GB storage, 90 ngày auto-delete nếu không active
- Web Service: Tự động sleep sau 15 phút không hoạt động
- Cold start: Mất 30-60 giây khi wake up từ sleep

### 3. Differences SQL Server vs PostgreSQL

Một số điểm khác biệt quan trọng:

| SQL Server | PostgreSQL |
|------------|------------|
| `NVARCHAR(MAX)` | `TEXT` hoặc `VARCHAR` |
| `DATETIME` | `TIMESTAMP` |
| `BIT` | `BOOLEAN` |
| `IDENTITY(1,1)` | `SERIAL` hoặc `GENERATED ALWAYS AS IDENTITY` |
| Case-insensitive | Case-sensitive (default) |
| `GETDATE()` | `NOW()` hoặc `CURRENT_TIMESTAMP` |

### 4. Entity Framework Core

Code hiện tại đã support cả 2 database:
```csharp
// Tự động detect PostgreSQL hoặc SQL Server based on connection string
var isPostgreSQL = connectionString?.Contains("Host=") ?? false;

if (isPostgreSQL)
{
    options.UseNpgsql(connectionString);
}
else
{
    options.UseSqlServer(connectionString);
}
```

## 🐛 Troubleshooting

### Build Failed
- Kiểm tra Dockerfile path đúng chưa
- Xem logs để biết lỗi cụ thể

### Cannot connect to database
- Kiểm tra CONNECTION_STRING có đúng không (Internal URL cho web service)
- Kiểm tra database đã được tạo chưa

### Application Error 
- Xem logs trong Render Dashboard
- Kiểm tra environment variables đã đủ chưa

### Migration Issues
- Xem các migration hiện tại: `dotnet ef migrations list`
- Remove migration nếu cần: `dotnet ef migrations remove`
- Tạo migration mới cho PostgreSQL

## 📞 Support

Nếu gặp vấn đề, check:
1. Render Dashboard > Logs
2. Render Dashboard > Events
3. PostgreSQL Database > Connections

## 🔗 Links hữu ích

- [Render Documentation](https://render.com/docs)
- [PostgreSQL on Render](https://render.com/docs/databases)
- [Docker Deploys](https://render.com/docs/docker)
- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
