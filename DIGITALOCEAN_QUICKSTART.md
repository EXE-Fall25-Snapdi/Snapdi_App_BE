# 🚀 Quick Start: Deploy Snapdi to DigitalOcean + Supabase

## Tổng quan nhanh

Hướng dẫn này giúp bạn deploy Snapdi Backend API lên **DigitalOcean App Platform** và kết nối với **Supabase PostgreSQL** trong 30 phút.

---

## ✅ Checklist nhanh

### 1. Chuẩn bị Database (10 phút)

- [ ] Tạo project trên [Supabase](https://supabase.com)
- [ ] Lấy connection string từ Settings → Database
- [ ] Chạy script khởi tạo database: `BE2/Snapdi_App_BE/Snapdi/Scripts/init-database-postgres.sql`
- [ ] Xác nhận tables đã được tạo trong Table Editor

📖 **Chi tiết**: Xem `SUPABASE_DATABASE_SETUP.md`

### 2. Chuẩn bị Code (5 phút)

- [ ] Push code lên GitHub repository
- [ ] Chuẩn bị environment variables từ file `.env`
- [ ] Copy connection string từ Supabase

### 3. Deploy lên DigitalOcean (15 phút)

- [ ] Tạo App mới trên [DigitalOcean App Platform](https://cloud.digitalocean.com/apps)
- [ ] Kết nối với GitHub repository
- [ ] Cấu hình:
  - Source: `BE2/Snapdi_App_BE/Snapdi`
  - Dockerfile: `BE2/Snapdi_App_BE/Snapdi/Dockerfile`
  - Port: `8080`
- [ ] Thêm environment variables
- [ ] Deploy và đợi build hoàn tất

📖 **Chi tiết**: Xem `DIGITALOCEAN_DEPLOYMENT_GUIDE.md`

### 4. Test API

- [ ] Truy cập Swagger UI: `https://your-app.ondigitalocean.app/swagger`
- [ ] Test API endpoint: `/api/users/roles`
- [ ] Kiểm tra database connection

---

## 🎯 Environment Variables cần thiết

Copy từ file `.env` của bạn hoặc tham khảo `.env.example`:

```bash
# Database (Supabase)
CONNECTION_STRING=Host=db.xxxxx.supabase.co;Database=postgres;Username=postgres;Password=xxx;SSL Mode=Require;Trust Server Certificate=true

# JWT
JWT_KEY=your-super-secret-jwt-key-minimum-32-characters-long
JWT_ISSUER=SnapdiAPI
JWT_AUDIENCE=SnapdiClient
JWT_EXPIRATION_HOURS=24

# App
APP_BASE_URL=${APP_URL}
ASPNETCORE_ENVIRONMENT=Production

# Cloudinary
CLOUDINARY_CLOUD_NAME=your_cloud_name
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret

# Email
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=your-email@gmail.com
FROM_NAME=Snapdi Team
```

---

## 📂 File Structure

```
Snapdi/
├── digitalocean-app.yaml              ← App Platform config (tùy chọn)
├── DIGITALOCEAN_DEPLOYMENT_GUIDE.md   ← Hướng dẫn chi tiết
├── DIGITALOCEAN_QUICKSTART.md         ← File này
└── BE2/
    └── Snapdi_App_BE/
        ├── SUPABASE_DATABASE_SETUP.md ← Hướng dẫn setup DB
        └── Snapdi/
            ├── Dockerfile              ← Docker build config
            ├── .env.example            ← Template environment variables
            └── Scripts/
                └── init-database-postgres.sql  ← Database schema
```

---

## 🔗 Links nhanh

- **Supabase Dashboard**: https://app.supabase.com
- **DigitalOcean Apps**: https://cloud.digitalocean.com/apps
- **Cloudinary Dashboard**: https://cloudinary.com/console
- **GitHub**: Push code của bạn lên đây

---

## 💡 Tips

1. **Test locally trước**: Chạy `docker-compose up` để test Docker build
2. **Secure secrets**: Đánh dấu các environment variables nhạy cảm là "Encrypted" trong DigitalOcean
3. **Monitor logs**: Xem Runtime Logs trong DigitalOcean để debug
4. **Database backup**: Supabase tự động backup hàng ngày (Free plan: 7 days)

---

## 🐛 Common Issues

### Build Failed
→ Kiểm tra Dockerfile path và source directory

### Cannot connect to database
→ Kiểm tra CONNECTION_STRING có đúng format và SSL Mode=Require

### JWT Key Error
→ JWT_KEY phải có ít nhất 32 ký tự

### Health Check Failed
→ Tăng initial_delay_seconds lên 60-90 seconds

---

## 📞 Support

- Xem logs chi tiết trong DigitalOcean Dashboard
- Check database logs trong Supabase Dashboard
- Đọc hướng dẫn chi tiết trong `DIGITALOCEAN_DEPLOYMENT_GUIDE.md`

---

## ✨ Sau khi Deploy thành công

API của bạn sẽ có:
- ✅ HTTPS với SSL certificate miễn phí
- ✅ Tự động deploy khi push code mới
- ✅ Health monitoring và auto-restart
- ✅ Swagger UI tại `/swagger`
- ✅ SignalR hubs tại `/hubs/chat` và `/hubs/booking`

**URL mẫu**: `https://snapdi-backend-xxxxx.ondigitalocean.app`

---

Chúc bạn deploy thành công! 🎉


