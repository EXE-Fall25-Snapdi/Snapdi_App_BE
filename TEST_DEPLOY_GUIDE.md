# 🧪 Hướng dẫn Test Deploy nhánh test-deploy

## ✅ Đã hoàn thành

✔️ Tạo nhánh `test-deploy` từ nhánh `kietnt`  
✔️ Commit tất cả thay đổi về Docker và PostgreSQL  
✔️ Push lên GitHub repository  

**Branch**: `test-deploy`  
**Commit**: feat: Add Docker and PostgreSQL support for Render deployment

## 🔗 GitHub Links

**Repository**: https://github.com/EXE-Fall25-Snapdi/Snapdi_App_BE

**Create Pull Request**: https://github.com/EXE-Fall25-Snapdi/Snapdi_App_BE/pull/new/test-deploy

**Branch URL**: https://github.com/EXE-Fall25-Snapdi/Snapdi_App_BE/tree/test-deploy

## 📋 Files đã commit (15 files)

### Mới thêm:
1. ✅ `RENDER_DEPLOYMENT_GUIDE.md` - Hướng dẫn deploy lên Render
2. ✅ `SETUP_SUMMARY.md` - Tổng hợp tất cả thay đổi
3. ✅ `Snapdi/.env.example` - Template environment variables
4. ✅ `Snapdi/DEPLOYMENT_CHECKLIST.md` - Deployment checklist
5. ✅ `Snapdi/DOCKER_GUIDE.md` - Docker usage guide
6. ✅ `Snapdi/Dockerfile` - Docker configuration
7. ✅ `Snapdi/Snapdi.Api/Controllers/HealthController.cs` - Health check endpoints
8. ✅ `Snapdi/docker-compose.yml` - Docker Compose setup
9. ✅ `Snapdi/migrate.ps1` - Migration helper (Windows)
10. ✅ `Snapdi/migrate.sh` - Migration helper (Linux/Mac)
11. ✅ `setup.ps1` - Quick setup wizard

### Đã cập nhật:
12. ✅ `Snapdi/.gitignore` - Ignore Docker files
13. ✅ `Snapdi/Snapdi.Api/Program.cs` - Auto-detect database type
14. ✅ `Snapdi/Snapdi.Api/Snapdi.Api.csproj` - Add Npgsql packages
15. ✅ `Snapdi/Snapdi.Repositories/Snapdi.Repositories.csproj` - Add PostgreSQL support

## 🧪 Test Local trước khi Deploy

### Option 1: Test với Docker Compose (Recommended)

```powershell
# Di chuyển vào thư mục project
cd C:\Users\enteecaay\Desktop\Snapdi\BE2\Snapdi_App_BE\Snapdi

# Build và start containers
docker-compose up --build

# Test API
# Mở browser: http://localhost:8080/swagger
```

### Option 2: Test với PostgreSQL riêng lẻ

```powershell
# Start PostgreSQL only
docker-compose up -d postgres

# Copy .env template
cd Snapdi
copy .env.example .env

# Edit .env và set:
# CONNECTION_STRING=Host=localhost;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123

# Run migrations
.\migrate.ps1
# Chọn option 2 (Apply migrations)

# Run API
cd Snapdi.Api
dotnet run

# Test API
# Mở browser: https://localhost:7000/swagger
```

### Test Checklist

- [ ] API starts successfully
- [ ] Swagger UI loads
- [ ] Health check endpoint works: `GET /api/health`
- [ ] Detailed health check works: `GET /api/health/detailed`
- [ ] Database connection is healthy
- [ ] Can register new user
- [ ] Can login and get JWT token
- [ ] Protected endpoints work with JWT

## 🚀 Deploy to Render

Sau khi test local thành công, làm theo các bước sau để deploy lên Render:

### Bước 1: Tạo PostgreSQL Database

1. Login vào [Render Dashboard](https://dashboard.render.com)
2. Click **New +** → **PostgreSQL**
3. Điền thông tin:
   - Name: `snapdi-database-test`
   - Database: `snapdi_db`
   - User: `snapdi_user`
   - Region: `Singapore`
   - Plan: `Free`
4. Click **Create Database**
5. **Copy Internal Database URL** (sẽ dùng cho web service)

### Bước 2: Tạo Web Service

1. Click **New +** → **Web Service**
2. Connect GitHub repository: `EXE-Fall25-Snapdi/Snapdi_App_BE`
3. Chọn branch: **test-deploy** ⚠️ (quan trọng!)
4. Điền configuration:

```
Name: snapdi-api-test
Region: Singapore
Branch: test-deploy
Root Directory: Snapdi_App_BE/Snapdi
Environment: Docker
Dockerfile Path: ./Dockerfile
```

5. Thêm **Environment Variables**:

```bash
CONNECTION_STRING=<paste Internal Database URL từ bước 1>

JWT_KEY=test-super-secret-jwt-key-minimum-32-characters-long-for-testing

JWT_ISSUER=SnapdiAPI

JWT_AUDIENCE=SnapdiClient

JWT_EXPIRATION_HOURS=24

APP_BASE_URL=https://snapdi-api-test.onrender.com

ASPNETCORE_ENVIRONMENT=Production
```

6. Click **Create Web Service**

### Bước 3: Monitor Deployment

1. Xem **Logs** tab để theo dõi build process
2. Build sẽ mất khoảng 5-10 phút
3. Khi deploy xong, click vào URL của service
4. Test Swagger UI: `https://snapdi-api-test.onrender.com/swagger`

### Bước 4: Run Database Migrations

Từ máy local, run migrations lên production database:

```powershell
# Copy External Database URL từ Render PostgreSQL dashboard

# Set environment variable
$env:CONNECTION_STRING="<paste External Database URL>"

# Navigate to project
cd C:\Users\enteecaay\Desktop\Snapdi\BE2\Snapdi_App_BE\Snapdi\Snapdi.Api

# Run migrations
dotnet ef database update --project ../Snapdi.Repositories

# Hoặc dùng migration script
cd ..
.\migrate.ps1
# Chọn option 2
```

### Bước 5: Test Production API

Test các endpoints:

```bash
# Base URL
https://snapdi-api-test.onrender.com

# Swagger UI
https://snapdi-api-test.onrender.com/swagger

# Health check
https://snapdi-api-test.onrender.com/api/health

# Detailed health check
https://snapdi-api-test.onrender.com/api/health/detailed
```

## 📊 Verify Deployment Success

### ✅ Checklist

- [ ] Service shows "Live" status in Render Dashboard
- [ ] Swagger UI loads successfully
- [ ] Health check returns status "healthy"
- [ ] Database connection shows as "healthy" in detailed health check
- [ ] Can register new user via API
- [ ] Can login and receive JWT token
- [ ] Protected endpoints work with JWT authorization

### 🐛 Troubleshooting

#### Build Failed
**Check:**
- Logs tab for specific errors
- Dockerfile path: `./Dockerfile`
- Root directory: `Snapdi_App_BE/Snapdi`
- Branch: `test-deploy`

#### Application Error
**Check:**
- All environment variables are set
- CONNECTION_STRING format is correct (PostgreSQL format)
- JWT_KEY is at least 32 characters
- Logs for specific error messages

#### Database Connection Failed
**Check:**
- Using **Internal Database URL** (not External)
- Database is in same region as web service
- Connection string includes SSL mode

## 📝 After Testing

### If test successful ✅

Có thể merge nhánh `test-deploy` vào `kietnt` hoặc `main`:

```powershell
# Switch về nhánh chính
git checkout kietnt

# Merge test-deploy
git merge test-deploy

# Push lên GitHub
git push origin kietnt
```

### If need changes ❌

```powershell
# Make changes on test-deploy branch
git checkout test-deploy

# Edit files...

# Commit và push
git add .
git commit -m "fix: Your fix description"
git push origin test-deploy

# Render sẽ tự động deploy lại
```

## 🔗 Useful Links

- **Render Dashboard**: https://dashboard.render.com
- **Documentation**: 
  - [DOCKER_GUIDE.md](./Snapdi/DOCKER_GUIDE.md)
  - [RENDER_DEPLOYMENT_GUIDE.md](./RENDER_DEPLOYMENT_GUIDE.md)
  - [DEPLOYMENT_CHECKLIST.md](./Snapdi/DEPLOYMENT_CHECKLIST.md)
  - [SETUP_SUMMARY.md](./SETUP_SUMMARY.md)

## 💡 Tips

1. **Free Plan Sleep**: Service sẽ sleep sau 15 phút không hoạt động, wake up mất ~30-60 giây
2. **Database Backup**: Free database sẽ bị xóa sau 90 ngày không active
3. **Logs**: Luôn check logs khi có lỗi
4. **Environment**: Có thể update environment variables bất cứ lúc nào trong Render Dashboard

---

**🎉 Chúc bạn test deploy thành công!**

Nếu có vấn đề, check documentation hoặc xem logs trong Render Dashboard.
