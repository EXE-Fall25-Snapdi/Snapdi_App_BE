# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Snapdi.Api/Snapdi.Api.csproj", "Snapdi.Api/"]
COPY ["Snapdi.Services/Snapdi.Services.csproj", "Snapdi.Services/"]
COPY ["Snapdi.Repositories/Snapdi.Repositories.csproj", "Snapdi.Repositories/"]

RUN dotnet restore "Snapdi.Api/Snapdi.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Snapdi.Api"
RUN dotnet build "Snapdi.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Snapdi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Snapdi.Api.dll"]
