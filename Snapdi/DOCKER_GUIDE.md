# Docker Setup Guide for Snapdi Backend API

## 📦 Prerequisites

- Docker Desktop installed (Windows/Mac) hoặc Docker Engine (Linux)
- .NET 8.0 SDK (chỉ cho development)
- Git

## 🚀 Quick Start

### 1. Clone và Setup

```bash
cd BE2/Snapdi_App_BE/Snapdi

# Copy environment file
cp .env.example .env

# Edit .env file với thông tin của bạn (optional, có default values)
```

### 2. Chạy với Docker Compose

```bash
# Build và start tất cả services
docker-compose up --build

# Hoặc chạy ở background
docker-compose up -d --build
```

API sẽ chạy tại: **http://localhost:8080**  
Swagger UI: **http://localhost:8080/swagger**

PostgreSQL Database:

- Host: localhost
- Port: 5432
- Database: snapdi_db
- Username: snapdi_user
- Password: snapdi_password_123

### 3. Stop Services

```bash
# Stop và remove containers
docker-compose down

# Stop, remove containers và volumes (xóa data)
docker-compose down -v
```

## 🔧 Advanced Usage

### Build Dockerfile riêng lẻ

```bash
# Build image
docker build -t snapdi-api:latest .

# Run container
docker run -p 8080:8080 `
  -e CONNECTION_STRING="Host=host.docker.internal;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123" `
  -e JWT_KEY="your-super-secret-jwt-key-minimum-32-characters-long" `
  snapdi-api:latest
```

### View Logs

```bash
# Xem logs của tất cả services
docker-compose logs

# Xem logs của API service
docker-compose logs api

# Follow logs real-time
docker-compose logs -f api
```

### Execute Commands trong Container

```bash
# Access API container shell
docker-compose exec api /bin/bash

# Access PostgreSQL
docker-compose exec postgres psql -U snapdi_user -d snapdi_db
```

### Restore Database từ SQL dump

```bash
# Copy SQL file vào container
docker cp your_backup.sql snapdi-postgres:/tmp/

# Restore
docker-compose exec postgres psql -U snapdi_user -d snapdi_db -f /tmp/your_backup.sql
```

## 🗄️ Database Migrations

### Option 1: Sử dụng Migration Script

```powershell
# Windows PowerShell
.\migrate.ps1
```

```bash
# Linux/Mac
chmod +x migrate.sh
./migrate.sh
```

### Option 2: Manual EF Core Commands

```bash
# Install EF Core tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# Tạo migration mới
cd Snapdi.Api
dotnet ef migrations add InitialPostgreSQL --project ../Snapdi.Repositories

# Apply migrations
dotnet ef database update --project ../Snapdi.Repositories

# List migrations
dotnet ef migrations list --project ../Snapdi.Repositories
```

## 🔄 Development Workflow

### 1. Làm việc với SQL Server (Local Development)

Update `.env`:

```env
CONNECTION_STRING=Data Source=.;Initial Catalog=Snapdi_DB_v2u1;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=False
```

### 2. Test với PostgreSQL (Docker)

Update `.env`:

```env
CONNECTION_STRING=Host=localhost;Port=5432;Database=snapdi_db;Username=snapdi_user;Password=snapdi_password_123
```

Start PostgreSQL:

```bash
docker-compose up postgres -d
```

Run API từ Visual Studio hoặc:

```bash
cd Snapdi.Api
dotnet run
```

### 3. Full Docker Stack

```bash
docker-compose up --build
```

## 📊 Health Checks

API có health check endpoint tại: `/health` (nếu đã implement)

Check PostgreSQL health:

```bash
docker-compose exec postgres pg_isready -U snapdi_user
```

## 🐛 Troubleshooting

### Port đã được sử dụng

Nếu port 8080 hoặc 5432 đã được sử dụng, edit `docker-compose.yml`:

```yaml
services:
  postgres:
    ports:
      - "5433:5432" # Đổi sang port khác

  api:
    ports:
      - "8081:8080" # Đổi sang port khác
```

### Cannot connect to database

Kiểm tra:

1. PostgreSQL container đã running chưa: `docker-compose ps`
2. Logs của database: `docker-compose logs postgres`
3. Connection string đúng chưa

### Build failed

```bash
# Clean và rebuild
docker-compose down
docker-compose build --no-cache
docker-compose up
```

### Database connection timeout

Đợi PostgreSQL container khởi động hoàn toàn (khoảng 10-15 giây). API có health check sẽ tự retry.

## 📝 Environment Variables

Các biến environment quan trọng:

| Variable                 | Description                    | Default               |
| ------------------------ | ------------------------------ | --------------------- |
| `CONNECTION_STRING`      | Database connection string     | PostgreSQL local      |
| `JWT_KEY`                | JWT signing key (min 32 chars) | -                     |
| `JWT_ISSUER`             | JWT token issuer               | SnapdiAPI             |
| `JWT_AUDIENCE`           | JWT token audience             | SnapdiClient          |
| `JWT_EXPIRATION_HOURS`   | Token expiration time          | 24                    |
| `APP_BASE_URL`           | Application base URL           | http://localhost:8080 |
| `ASPNETCORE_ENVIRONMENT` | Environment name               | Development           |

## 🔐 Security Notes

**⚠️ QUAN TRỌNG:**

1. **Không commit file `.env`** vào Git
2. Đổi `JWT_KEY` trong production (min 32 characters)
3. Đổi PostgreSQL password trong production
4. Sử dụng SSL cho database connection trong production

## 📚 Useful Commands

```bash
# Rebuild specific service
docker-compose up -d --build api

# View container resource usage
docker stats

# Remove all stopped containers
docker container prune

# Remove all unused images
docker image prune -a

# Full cleanup (containers, volumes, images)
docker system prune -a --volumes
```

## 🎯 Production Deployment

Xem file `RENDER_DEPLOYMENT_GUIDE.md` để deploy lên Render.

Các platform khác:

- **AWS ECS**: Use the Dockerfile
- **Azure Container Apps**: Use the Dockerfile
- **Google Cloud Run**: Use the Dockerfile
- **Heroku**: Use container registry
- **DigitalOcean App Platform**: Connect GitHub repo

## 📞 Support

Nếu gặp vấn đề:

1. Xem logs: `docker-compose logs -f`
2. Check health: `docker-compose ps`
3. Restart services: `docker-compose restart`
