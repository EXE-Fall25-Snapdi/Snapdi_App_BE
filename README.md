# Snapdi API - Backend Application

Modern photography booking platform API built with .NET 8.0, PostgreSQL (Supabase), and deployed on Google Cloud Run.

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Google Cloud SDK](https://cloud.google.com/sdk/docs/install)
- PostgreSQL database (Supabase recommended)

### Local Development

1. **Clone the repository**
   ```bash
   cd BE2/Snapdi_App_BE/Snapdi
   ```

2. **Configure environment**
   ```bash
   cd Snapdi.Api
   copy env.template .env
   # Edit .env with your configuration
   ```

3. **Run the application**
   ```bash
   dotnet restore
   dotnet run --project Snapdi.Api
   ```

4. **Access Swagger UI**
   ```
   https://localhost:7000/swagger
   ```

## 🌐 Deployment to Google Cloud Run

### Option 1: Automated Deployment (Recommended)

```powershell
cd BE2\Snapdi_App_BE
.\deploy-cloudrun.ps1 -ProjectId "your-gcp-project-id"
```

### Option 2: Manual Deployment

See detailed guides:
- **[DEPLOYMENT_SUMMARY.md](DEPLOYMENT_SUMMARY.md)** - Start here
- **[CLOUDRUN_QUICKSTART.md](CLOUDRUN_QUICKSTART.md)** - Quick commands
- **[GOOGLE_CLOUD_RUN_DEPLOYMENT.md](GOOGLE_CLOUD_RUN_DEPLOYMENT.md)** - Complete guide

## 📁 Project Structure

```
Snapdi_App_BE/
├── deploy-cloudrun.ps1                 # Automated Cloud Run deployment
├── DEPLOYMENT_SUMMARY.md               # Deployment overview
├── CLOUDRUN_QUICKSTART.md              # Quick reference
├── GOOGLE_CLOUD_RUN_DEPLOYMENT.md      # Complete deployment guide
├── SUPABASE_DATABASE_SETUP.md          # Database setup guide
└── Snapdi/
    ├── Snapdi.Api/                     # ASP.NET Core Web API
    │   ├── Controllers/                # API endpoints
    │   ├── Hubs/                       # SignalR hubs (chat, booking)
    │   ├── Services/                   # JWT service
    │   ├── appsettings.json            # Configuration
    │   └── env.template                # Environment variables template
    ├── Snapdi.Services/                # Business logic layer
    │   ├── Services/                   # Service implementations
    │   ├── DTOs/                       # Data transfer objects
    │   └── Interfaces/                 # Service contracts
    ├── Snapdi.Repositories/            # Data access layer
    │   ├── Repositories/               # Repository implementations
    │   ├── Models/                     # Entity models
    │   ├── Context/                    # EF Core DbContext
    │   └── Migrations/                 # Database migrations
    ├── Scripts/
    │   └── init-database-postgres.sql  # Database initialization script
    ├── Dockerfile                      # Docker container configuration
    ├── .gcloudignore                   # Cloud deployment exclusions
    └── ENV_VARIABLES.md                # Environment variables documentation
```

## 🛠️ Tech Stack

- **Framework**: .NET 8.0 / ASP.NET Core
- **Database**: PostgreSQL (Supabase)
- **ORM**: Entity Framework Core with Npgsql
- **Authentication**: JWT Bearer tokens
- **Real-time**: SignalR (WebSockets)
- **Image Storage**: Cloudinary
- **Email**: SMTP (Gmail)
- **Documentation**: Swagger/OpenAPI
- **Deployment**: Google Cloud Run (containerized)

## 🔧 Key Features

### API Endpoints
- ✅ **Authentication** - JWT-based auth, registration, password reset
- ✅ **Users** - User management, roles (Admin, Customer, Photographer)
- ✅ **Photographers** - Profile, portfolio, styles, photo types
- ✅ **Bookings** - Create, manage, track booking status
- ✅ **Payments** - Payment processing and tracking
- ✅ **Reviews** - Customer reviews and ratings
- ✅ **Messaging** - Real-time chat via SignalR
- ✅ **Blogs** - Blog posts with keywords
- ✅ **Vouchers** - Discount vouchers and usage tracking
- ✅ **Dashboard** - Analytics and statistics

### Real-time Features (SignalR)
- **Chat Hub** (`/hubs/chat`) - Real-time messaging
- **Booking Hub** (`/hubs/booking`) - Live booking updates

### Image Management
- Cloudinary integration for image uploads
- Portfolio and profile image management
- Optimized image delivery

## 📊 Database

### Supabase PostgreSQL

The application uses Supabase (PostgreSQL) as the database.

**Setup Instructions**: See [SUPABASE_DATABASE_SETUP.md](SUPABASE_DATABASE_SETUP.md)

**Connection String**:
```
Host=db.uagjvfntfxtttqyvekes.supabase.co;Database=postgres;Username=postgres;Password=$Snapdi$2025;SSL Mode=Require;Trust Server Certificate=true
```

### Database Schema

22 tables including:
- User, Role, PhotographerProfile
- Booking, BookingStatus, Payment, PaymentStatus
- Review, Conversation, Message
- Blog, Keyword, PhotoPortfolio
- Style, PhotoType, Voucher
- And more...

## 🔐 Environment Variables

Create `.env` file in `Snapdi/Snapdi.Api/.env`:

```bash
# Required
CONNECTION_STRING=<supabase-connection-string>
JWT_KEY=<minimum-32-characters>
CLOUDINARY_CLOUD_NAME=<your-cloudinary-account>
CLOUDINARY_API_KEY=<your-api-key>
CLOUDINARY_API_SECRET=<your-api-secret>

# Optional
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=<your-email>
SMTP_PASSWORD=<your-app-password>
```

See [ENV_VARIABLES.md](Snapdi/ENV_VARIABLES.md) for complete list.

## 🐳 Docker

### Build Image
```bash
cd Snapdi
docker build -t snapdi-api .
```

### Run Container
```bash
docker run -p 8080:8080 \
  -e CONNECTION_STRING="<connection-string>" \
  -e JWT_KEY="<your-jwt-key>" \
  snapdi-api
```

### Docker Compose
```bash
docker-compose up
```

## 📝 API Documentation

Access Swagger UI at: `https://your-api-url/swagger`

- Complete API documentation
- Interactive testing
- Schema definitions
- Authentication support

## 🧪 Testing

```bash
# Run tests
dotnet test

# Test specific endpoint
curl -X POST https://your-api-url/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@snapdi.com","password":"Admin@123"}'
```

## 📈 Monitoring

### Cloud Run Logs
```bash
gcloud run services logs read snapdi-api --region=asia-southeast1 --follow
```

### Health Check
```bash
curl https://your-api-url/api/health
```

## 💰 Cost Estimate

**Google Cloud Run** (with $300 free credit):
- Free tier: 2M requests/month
- Estimated cost: $0-1/month for moderate usage
- Your $300 credit covers months of development

**Supabase**:
- Free tier: 500MB database, 1GB file storage
- Plenty for development and small-scale production

## 📚 Documentation

| File | Purpose |
|------|---------|
| [DEPLOYMENT_SUMMARY.md](DEPLOYMENT_SUMMARY.md) | Deployment overview and next steps |
| [CLOUDRUN_QUICKSTART.md](CLOUDRUN_QUICKSTART.md) | Quick deployment commands |
| [GOOGLE_CLOUD_RUN_DEPLOYMENT.md](GOOGLE_CLOUD_RUN_DEPLOYMENT.md) | Complete deployment guide |
| [SUPABASE_DATABASE_SETUP.md](SUPABASE_DATABASE_SETUP.md) | Database setup instructions |
| [ENV_VARIABLES.md](Snapdi/ENV_VARIABLES.md) | Environment variables reference |

## 🔗 Related Projects

- **Mobile App**: `MobileApp/Snapdi_App_FE/` - Flutter mobile application
- **Web App**: `web/Snapdi_Web/` - React web application

## 🆘 Troubleshooting

### Common Issues

1. **Connection to Supabase failed**
   - Verify SSL Mode=Require in connection string
   - Check database is running in Supabase dashboard

2. **JWT Key too short**
   - Must be at least 32 characters
   - Generate: `[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))`

3. **Docker build failed**
   - Ensure Docker Desktop is running
   - Clear cache: `docker builder prune -a`

4. **Cloud Run deployment failed**
   - Check gcloud authentication: `gcloud auth list`
   - Verify project ID: `gcloud config get-value project`

See full troubleshooting guide in [GOOGLE_CLOUD_RUN_DEPLOYMENT.md](GOOGLE_CLOUD_RUN_DEPLOYMENT.md#troubleshooting)

## 📄 License

[Your License Here]

## 👥 Team

Snapdi Development Team

---

**Status**: ✅ Production Ready

**Last Updated**: October 29, 2025

**Deployment**: Google Cloud Run + Supabase