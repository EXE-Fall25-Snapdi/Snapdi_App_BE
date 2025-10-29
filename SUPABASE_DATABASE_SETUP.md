# Hướng dẫn Khởi tạo Database Supabase cho Snapdi

## 📋 Tổng quan

Tài liệu này hướng dẫn chi tiết cách khởi tạo và cấu hình PostgreSQL database trên Supabase cho dự án Snapdi.

---

## 🔧 Bước 1: Tạo Project trên Supabase

### 1.1. Truy cập Supabase

1. Mở https://supabase.com
2. Đăng nhập hoặc tạo tài khoản mới (miễn phí)
3. Click **"New project"**

### 1.2. Cấu hình Project

Điền thông tin project:

```
Name: snapdi-database
Database Password: $Snapdi$2025
Region: Southeast Asia (Singapore)
Pricing Plan: Free (hoặc Pro nếu cần)
```

> **Lưu ý**: Lưu lại password này, bạn sẽ cần dùng để connect tới database.

### 1.3. Đợi Project khởi tạo

Quá trình này mất khoảng 2-3 phút. Supabase sẽ:
- Tạo PostgreSQL database
- Cấu hình PostGIS extension
- Setup API endpoints
- Tạo SSL certificates

---

## 🔗 Bước 2: Lấy Connection String

### 2.1. Truy cập Database Settings

1. Trong Supabase Dashboard, click vào project vừa tạo
2. Vào **Settings** (icon bánh răng bên trái)
3. Chọn **Database**

### 2.2. Copy Connection Info

Tìm phần **"Connection string"**:

**URI Format (PostgreSQL):**
```
postgresql://postgres:[YOUR-PASSWORD]@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres
```

**Connection Parameters:**
```
Host: db.uagjvfntfxtttqyvekes.supabase.co
Port: 5432
Database: postgres
User: postgres
Password: $Snapdi$2025
```

### 2.3. Format cho .NET Connection String

Chuyển đổi sang format .NET (sử dụng trong `appsettings.json` hoặc environment variable):

```
Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true
```

> **Quan trọng**: Phải có `SSL Mode=Require` khi connect tới Supabase.

---

## 📊 Bước 3: Khởi tạo Database Schema

### Option 1: Sử dụng Supabase SQL Editor (Khuyên dùng)

#### 3.1. Mở SQL Editor

1. Trong Supabase Dashboard, click **"SQL Editor"** (icon database bên trái)
2. Click **"New query"**

#### 3.2. Load và Execute Script

1. Mở file `BE2/Snapdi_App_BE/Snapdi/Scripts/init-database-postgres.sql` trên máy local
2. Copy toàn bộ nội dung file
3. Paste vào SQL Editor trên Supabase
4. Click **"Run"** (hoặc Ctrl+Enter)

#### 3.3. Kiểm tra kết quả

Script sẽ tạo:
- ✅ 22 tables
- ✅ Foreign key relationships
- ✅ Indexes
- ✅ Seed data (roles, statuses, admin user)

Xem message ở cuối: `Database initialization completed successfully`

### Option 2: Sử dụng psql (Command Line)

#### 3.1. Cài đặt PostgreSQL Client

**Windows:**
```bash
# Download từ https://www.postgresql.org/download/windows/
# Hoặc sử dụng chocolatey
choco install postgresql
```

**macOS:**
```bash
brew install postgresql
```

**Linux:**
```bash
sudo apt-get install postgresql-client
```

#### 3.2. Connect và Execute

```bash
# Di chuyển tới thư mục chứa script
cd BE2/Snapdi_App_BE/Snapdi/Scripts

# Connect và execute script
psql "postgresql://postgres:$Snapdi$2025@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres?sslmode=require" -f init-database-postgres.sql
```

### Option 3: Sử dụng pgAdmin 4

#### 3.1. Tạo Server Connection

1. Mở pgAdmin 4
2. Right-click **"Servers"** → **"Create"** → **"Server"**
3. Điền thông tin:

**General Tab:**
```
Name: Supabase Snapdi
```

**Connection Tab:**
```
Host: db.uagjvfntfxtttqyvekes.supabase.co
Port: 5432
Maintenance database: postgres
Username: postgres
Password: $Snapdi$2025
```

**SSL Tab:**
```
SSL mode: Require
```

4. Click **"Save"**

#### 3.2. Execute Script

1. Expand server → Databases → postgres
2. Click **"Query Tool"** (icon SQL)
3. **File** → **Open** → Select `init-database-postgres.sql`
4. Click **"Execute"** (F5)

---

## 🗂️ Bước 4: Xác nhận Tables đã được tạo

### 4.1. Sử dụng Table Editor

1. Trong Supabase Dashboard, click **"Table Editor"**
2. Bạn sẽ thấy tất cả tables đã được tạo:

#### Core Tables:
- ✅ **User** - Thông tin người dùng (customer, photographer, admin)
- ✅ **Role** - Roles (Admin, Customer, Photographer)
- ✅ **PhotographerProfile** - Profile của photographer

#### Booking System:
- ✅ **Booking** - Đặt lịch chụp ảnh
- ✅ **BookingStatus** - Trạng thái booking
- ✅ **Payment** - Thanh toán
- ✅ **PaymentStatus** - Trạng thái thanh toán
- ✅ **Review** - Đánh giá sau khi hoàn thành

#### Content & Media:
- ✅ **Blog** - Blog posts
- ✅ **Keyword** - Keywords cho blog
- ✅ **KeywordsInBlog** - Mapping blog-keyword
- ✅ **PhotoPortfolio** - Portfolio của photographer

#### Messaging:
- ✅ **Conversation** - Cuộc trò chuyện
- ✅ **ConversationParticipant** - Người tham gia
- ✅ **Message** - Tin nhắn

#### Categories & Settings:
- ✅ **Style** - Phong cách chụp ảnh
- ✅ **PhotoType** - Loại ảnh (portrait, wedding, event, etc.)
- ✅ **PhotographerStyle** - Styles của photographer
- ✅ **PhotographerPhotoType** - Photo types của photographer với giá
- ✅ **Voucher** - Mã giảm giá
- ✅ **VoucherUsage** - Lịch sử sử dụng voucher
- ✅ **FeePolicy** - Chính sách phí platform

### 4.2. Check Seed Data

Xác nhận seed data đã được insert:

```sql
-- Check roles
SELECT * FROM "Role";
-- Expected: ADMIN, CUSTOMER, PHOTOGRAPHER

-- Check booking statuses
SELECT * FROM "BookingStatus";
-- Expected: Pending, Confirmed, Paid, Going, Processing, Done, Completed

-- Check payment statuses
SELECT * FROM "PaymentStatus";
-- Expected: Pending, Paid, Refunded, Confirmed

-- Check default admin user
SELECT * FROM "User" WHERE "Email" = 'admin@snapdi.com';
```

---

## 🔐 Bước 5: Cấu hình Security và Access

### 5.1. Row Level Security (RLS)

Supabase tự động bật RLS. Để API backend có thể access:

1. Vào **Authentication** → **Policies**
2. Tạm thời disable RLS cho testing:

```sql
-- Disable RLS cho tất cả tables (chỉ dùng cho testing/development)
ALTER TABLE "User" DISABLE ROW LEVEL SECURITY;
ALTER TABLE "Booking" DISABLE ROW LEVEL SECURITY;
ALTER TABLE "Payment" DISABLE ROW LEVEL SECURITY;
-- ... (disable cho tất cả tables)
```

> **Lưu ý Production**: Trong production, nên enable RLS và tạo policies phù hợp.

### 5.2. Database Roles

Supabase có sẵn role `postgres` với full permissions. API backend sẽ dùng role này.

---

## 🧪 Bước 6: Test Connection từ Local

### 6.1. Update appsettings.json

Trong file `BE2/Snapdi_App_BE/Snapdi/Snapdi.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 6.2. Test Local API

```bash
cd BE2/Snapdi_App_BE/Snapdi/Snapdi.Api
dotnet run
```

Mở browser: `https://localhost:7000/swagger`

Test một API endpoint:
- GET `/api/users/roles` - Lấy danh sách roles
- Kết quả mong đợi: JSON array với 3 roles

### 6.3. Test với Docker

```bash
cd BE2/Snapdi_App_BE/Snapdi

# Update CONNECTION_STRING trong docker-compose.yml
# Sau đó run:
docker-compose up
```

---

## 📊 Bước 7: Import Sample Data (Tùy chọn)

### 7.1. Sử dụng Sample Data Script

Nếu có file `Snapdi_DB_Sample_v2u1.sql` với sample data:

```bash
psql "postgresql://postgres:$Snapdi$2025@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres?sslmode=require" -f path/to/Snapdi_DB_Sample_v2u1.sql
```

### 7.2. Seed Data từ API

Hoặc tạo sample data qua API endpoints sau khi deploy.

---

## 🔄 Bước 8: Migrations và Updates

### 8.1. Entity Framework Migrations

Nếu có thay đổi schema trong tương lai:

```bash
cd BE2/Snapdi_App_BE/Snapdi/Snapdi.Api

# Tạo migration mới
dotnet ef migrations add MigrationName --project ../Snapdi.Repositories

# Apply migration lên Supabase
dotnet ef database update --project ../Snapdi.Repositories --connection "Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true"
```

### 8.2. Manual Schema Updates

Thực hiện trực tiếp trên Supabase SQL Editor:

```sql
-- Example: Add new column
ALTER TABLE "User" ADD COLUMN "LastLoginAt" TIMESTAMP;

-- Example: Create new index
CREATE INDEX idx_user_email ON "User"("Email");
```

---

## 📊 Bước 9: Monitoring và Backup

### 9.1. Database Monitoring

Trong Supabase Dashboard:
1. **Database** → **Health** - Xem CPU, memory, connections
2. **Database** → **Logs** - Xem query logs

### 9.2. Backups

**Free Plan:**
- Daily automated backups (kept for 7 days)
- Can restore from Supabase Dashboard

**Pro Plan:**
- Daily backups (kept for 30 days)
- Point-in-time recovery

### 9.3. Manual Backup

```bash
# Export entire database
pg_dump "postgresql://postgres:$Snapdi$2025@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres?sslmode=require" > snapdi_backup_$(date +%Y%m%d).sql

# Restore from backup
psql "postgresql://postgres:$Snapdi$2025@db.uagjvfntfxtttqyvekes.supabase.co:5432/postgres?sslmode=require" < snapdi_backup_20250101.sql
```

---

## 🐛 Troubleshooting

### Cannot connect to Supabase

**Lỗi**: `connection refused` hoặc `timeout`

**Giải pháp**:
1. Kiểm tra password có đúng không
2. Kiểm tra SSL Mode: phải là `Require`
3. Kiểm tra firewall/VPN không block port 5432
4. Test với psql client trước

### PostGIS Extension Error

**Lỗi**: `extension "postgis" does not exist`

**Giải pháp**:
```sql
-- Enable PostGIS extension
CREATE EXTENSION IF NOT EXISTS postgis;
```

### Foreign Key Constraint Failed

**Lỗi**: Khi insert data vi phạm foreign key

**Giải pháp**:
1. Insert data theo đúng thứ tự (parent tables trước)
2. Kiểm tra referenced records đã tồn tại
3. Check seed data đã được insert chưa

### Migration Failed

**Lỗi**: EF Core migration fail khi apply

**Giải pháp**:
1. Drop database và recreate (chỉ dùng cho dev)
2. Hoặc manually fix conflicts trong SQL Editor
3. Remove failed migration và tạo lại

---

## 📝 Checklist

- [ ] ✅ Đã tạo Supabase project
- [ ] ✅ Đã lấy connection string
- [ ] ✅ Đã execute init-database-postgres.sql thành công
- [ ] ✅ Tất cả 22 tables đã được tạo
- [ ] ✅ Seed data (roles, statuses, admin user) đã được insert
- [ ] ✅ Test connection từ local thành công
- [ ] ✅ API có thể query database thành công
- [ ] ✅ Đã disable RLS (cho development) hoặc configure policies (production)

---

## 🔗 Resources

- [Supabase Documentation](https://supabase.com/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [PostGIS Documentation](https://postgis.net/documentation/)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)

---

## 📞 Support

Nếu gặp vấn đề:
1. Check Supabase Dashboard → Database → Logs
2. Supabase Discord: https://discord.supabase.com
3. GitHub Issues: Report về repository

---

**Database URL cho reference:**
```
Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true
```


