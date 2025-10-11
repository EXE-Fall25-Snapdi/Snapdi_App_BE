# Database Migration Guide

## Cách c?p nh?t database khi pull code m?i

Khi b?n pull code t? repository và th?y có migration files m?i, hãy làm theo các b??c sau:

### B??c 1: Ki?m tra tr?ng thái migration hi?n t?i
```bash
dotnet ef migrations list --project Snapdi.Repositories --startup-project Snapdi.Api
```

### B??c 2: C?p nh?t database v?i migration m?i
```bash
dotnet ef database update --project Snapdi.Repositories --startup-project Snapdi.Api
```

### B??c 3: Ki?m tra build
```bash
dotnet build
```

## Migration History

### 20251011024614_InitialCreate
- Migration kh?i t?o cho database schema hi?n t?i

### 20251011024711_AddLastReadFieldsToConversationParticipant  
- Thêm `LastReadMessageId` (int?) và `LastReadAt` (datetime?) vào b?ng `ConversationParticipants`
- H? tr? tính n?ng ??m tin nh?n ch?a ??c trong conversation

## L?u ý quan tr?ng

1. **Backup database** tr??c khi ch?y migration trên production
2. **Ki?m tra connection string** trong `appsettings.json`
3. **??m b?o có quy?n ALTER TABLE** trên database
4. **Ch?y `dotnet build` tr??c** ?? ??m b?o code compile thành công

## Troubleshooting

### L?i "Invalid column name"
- Có th? migration ch?a ???c apply
- Ch?y l?i `dotnet ef database update`

### L?i "There is already an object named"  
- Migration có th? ?ã ???c apply th? công
- Ki?m tra b?ng `__EFMigrationsHistory` trong database

### L?i Entity Framework Tools
- ??m b?o có package `Microsoft.EntityFrameworkCore.Design` version 8.0.20
- Ch?y `dotnet restore` ?? c?p nh?t packages

## Contact
Liên h? team n?u g?p v?n ?? v?i migration.