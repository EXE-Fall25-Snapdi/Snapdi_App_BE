using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly SnapdiDbV2Context _context;
        private readonly IConfiguration _configuration;

        public HealthController(SnapdiDbV2Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Snapdi API"
            });
        }

        /// <summary>
        /// Detailed health check including database connection
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            var databaseCheck = await CheckDatabase();
            var configCheck = CheckConfiguration();

            var healthStatus = new HealthStatus
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Snapdi API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                checks = new HealthChecks
                {
                    database = databaseCheck,
                    configuration = configCheck
                }
            };

            var isHealthy = healthStatus.checks.database.status == "healthy" &&
                           healthStatus.checks.configuration.status == "healthy";

            if (!isHealthy)
            {
                return StatusCode(503, healthStatus);
            }

            return Ok(healthStatus);
        }

        private async Task<DatabaseCheckResult> CheckDatabase()
        {
            try
            {
                // Try to connect to database
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return new DatabaseCheckResult
                    {
                        status = "unhealthy",
                        message = "Cannot connect to database"
                    };
                }

                // Get database provider
                var provider = _context.Database.ProviderName;
                var isPostgreSQL = provider?.Contains("Npgsql") ?? false;
                var isSqlServer = provider?.Contains("SqlServer") ?? false;

                return new DatabaseCheckResult
                {
                    status = "healthy",
                    provider = isPostgreSQL ? "PostgreSQL" : isSqlServer ? "SQL Server" : "Unknown",
                    connected = true
                };
            }
            catch (Exception ex)
            {
                return new DatabaseCheckResult
                {
                    status = "unhealthy",
                    error = ex.Message
                };
            }
        }

        private ConfigurationCheckResult CheckConfiguration()
        {
            var missingConfigs = new List<string>();

            // Check required configurations
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")))
                missingConfigs.Add("JWT_KEY");

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING")))
                missingConfigs.Add("CONNECTION_STRING");

            if (missingConfigs.Any())
            {
                return new ConfigurationCheckResult
                {
                    status = "unhealthy",
                    missing = missingConfigs
                };
            }

            return new ConfigurationCheckResult
            {
                status = "healthy",
                message = "All required configurations are present"
            };
        }

        // Health status DTOs
        public class HealthStatus
        {
            public string status { get; set; }
            public DateTime timestamp { get; set; }
            public string service { get; set; }
            public string version { get; set; }
            public string environment { get; set; }
            public HealthChecks checks { get; set; }
        }

        public class HealthChecks
        {
            public DatabaseCheckResult database { get; set; }
            public ConfigurationCheckResult configuration { get; set; }
        }

        public class DatabaseCheckResult
        {
            public string status { get; set; }
            public string provider { get; set; }
            public bool? connected { get; set; }
            public string message { get; set; }
            public string error { get; set; }
        }

        public class ConfigurationCheckResult
        {
            public string status { get; set; }
            public List<string> missing { get; set; }
            public string message { get; set; }
        }
    }
}
