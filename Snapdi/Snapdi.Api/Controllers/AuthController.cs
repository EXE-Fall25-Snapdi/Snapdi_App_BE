using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Snapdi.Api.Services;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(IUserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// User login with email or phone number
        /// </summary>
        /// <remarks>
        /// Sample requests:
        /// 
        /// Login with email:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///         "emailOrPhone": "user@example.com",
        ///         "password": "password123"
        ///     }
        /// 
        /// Login with phone number:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///         "emailOrPhone": "+1234567890",
        ///         "password": "password123"
        ///     }
        /// 
        /// </remarks>
        /// <param name="loginDto">Login credentials containing email/phone and password</param>
        /// <returns>JWT token, refresh token, and user information if authentication is successful</returns>
        /// <response code="200">Login successful - returns tokens and user info</response>
        /// <response code="400">Invalid request format</response>
        /// <response code="401">Invalid credentials or account not active</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.AuthenticateAsync(loginDto.EmailOrPhone, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid email/phone or password");

            if (!user.IsActive)
                return Unauthorized("Account is not active");

            if (!user.IsVerify)
                return Unauthorized("Please verify your email address before logging in");

            var token = _jwtService.GenerateToken(user.UserId, user.Name, user.Email, user.RoleName);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userService.UpdateRefreshTokenAsync(user.UserId, refreshToken, refreshTokenExpiry);

            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = user
            });
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///         "name": "John Doe",
        ///         "email": "john@example.com",
        ///         "phone": "+1234567890",
        ///         "password": "securePassword123",
        ///         "roleId": 1,
        ///         "locationAddress": "123 Main St",
        ///         "locationCity": "New York",
        ///         "avatarUrl": "https://example.com/avatar.jpg"
        ///     }
        /// 
        /// Note: Only name, email, and password are required. Other fields are optional.
        /// After registration, a 6-digit verification code will be sent to your email automatically.
        /// Use the /api/auth/verify-email-code endpoint to verify your account.
        /// </remarks>
        /// <param name="createUserDto">User registration data</param>
        /// <returns>Created user information</returns>
        /// <response code="201">User created successfully - verification code sent via email</response>
        /// <response code="400">Validation error or email/phone already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _userService.IsEmailExistsAsync(createUserDto.Email))
                return BadRequest("Email already exists");

            // Check if phone already exists (if provided)
            if (!string.IsNullOrEmpty(createUserDto.Phone) && await _userService.IsPhoneExistsAsync(createUserDto.Phone))
                return BadRequest("Phone number already exists");

            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction("GetUser", "Users", new { id = user.UserId }, new { 
                User = user, 
                Message = "Registration successful. Please check your email for a 6-digit verification code." 
            });
        }

        /// <summary>
        /// Photographer registration with additional required fields
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/register-photographer
        ///     {
        ///         "name": "Jane Smith",
        ///         "email": "jane@example.com",
        ///         "phone": "+1234567890",
        ///         "password": "securePassword123",
        ///         "locationAddress": "123 Photography St",
        ///         "locationCity": "New York",
        ///         "yearsOfExperience": "5 years",
        ///         "equipmentDescription": "Canon EOS R5, Sony A7R IV, various lenses",
        ///         "description": "Professional wedding and portrait photographer",
        ///         "isAvailable": false,
        ///         "avatarUrl": "https://example.com/avatar.jpg"
        ///     }
        /// 
        /// Minimal request (without optional fields):
        /// 
        ///     POST /api/auth/register-photographer
        ///     {
        ///         "name": "Mobile Photographer",
        ///         "email": "mobile@example.com",
        ///         "password": "securePassword123",
        ///         "locationCity": "San Francisco",
        ///         "yearsOfExperience": "3 years",
        ///         "equipmentDescription": "Canon EOS R6, portable equipment"
        ///     }
        /// 
        /// Note: Required fields are name, email, password, locationCity, yearsOfExperience, and equipmentDescription.
        /// LocationAddress is optional - useful for mobile photographers or those who prefer not to share address.
        /// 
        /// IMPORTANT NOTES:
        /// - levelPhotographer will be set to null by default and can only be assigned by admin later
        /// - isAvailable defaults to false - photographers need admin approval before they can accept bookings
        /// - After registration, a 6-digit verification code will be sent to your email automatically
        /// - Use the /api/auth/verify-email-code endpoint to verify your account
        /// - Once verified, wait for admin to assign your photographer level and set availability
        /// </remarks>
        /// <param name="createPhotographerDto">Photographer registration data</param>
        /// <returns>Created photographer information with profile</returns>
        /// <response code="201">Photographer created successfully - verification code sent via email</response>
        /// <response code="400">Validation error or email/phone already exists</response>
        [HttpPost("register-photographer")]
        [ProducesResponseType(typeof(PhotographerRegistrationResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PhotographerRegistrationResponseDto>> RegisterPhotographer(CreatePhotographerDto createPhotographerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            if (await _userService.IsEmailExistsAsync(createPhotographerDto.Email))
                return BadRequest("Email already exists");

            // Check if phone already exists (if provided)
            if (!string.IsNullOrEmpty(createPhotographerDto.Phone) && await _userService.IsPhoneExistsAsync(createPhotographerDto.Phone))
                return BadRequest("Phone number already exists");

            var photographerUser = await _userService.CreatePhotographerAsync(createPhotographerDto);
            
            var response = new PhotographerRegistrationResponseDto
            {
                User = photographerUser,
                Message = "Photographer registration successful. Please check your email for a 6-digit verification code to verify your account. After verification, an admin will review and assign your photographer level before you can start accepting bookings."
            };

            return CreatedAtAction("GetUserWithPhotographer", "Users", new { id = photographerUser.UserId }, response);
        }

        /// <summary>
        /// Verify email address
        /// </summary>
        /// <param name="token">Email verification token from the verification email</param>
        /// <returns>Verification result</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired token</response>
        [HttpGet("verify-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Verification token is required");

            var result = await _userService.VerifyEmailAsync(token);
            if (!result)
                return BadRequest("Invalid or expired verification token");

            return Ok(new { Message = "Email verified successfully! You can now log in to your account." });
        }

        /// <summary>
        /// Verify email address (POST version)
        /// </summary>
        /// <param name="verifyEmailDto">Verification token data</param>
        /// <returns>Verification result</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired token</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> VerifyEmailPost(VerifyEmailDto verifyEmailDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.VerifyEmailAsync(verifyEmailDto.Token);
            if (!result)
                return BadRequest("Invalid or expired verification token");

            return Ok(new { Message = "Email verified successfully! You can now log in to your account." });
        }

        /// <summary>
        /// Verify email address using 6-digit code
        /// </summary>
        /// <param name="verifyEmailWithCodeDto">Email and verification code</param>
        /// <returns>Verification result</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired code</response>
        /// <response code="404">User not found</response>
        [HttpPost("verify-email-code")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> VerifyEmailWithCode(VerifyEmailWithCodeDto verifyEmailWithCodeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmailAsync(verifyEmailWithCodeDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsVerify)
                return BadRequest("Email is already verified");

            var result = await _userService.VerifyEmailWithCodeAsync(verifyEmailWithCodeDto.Email, verifyEmailWithCodeDto.Code);
            if (!result)
                return BadRequest("Invalid or expired verification code");

            return Ok(new { Message = "Email verified successfully! You can now log in to your account." });
        }

        /// <summary>
        /// Resend email verification
        /// </summary>
        /// <param name="resendVerificationDto">Email to resend verification to</param>
        /// <returns>Resend result</returns>
        /// <response code="200">Verification email sent</response>
        /// <response code="400">Invalid email or already verified</response>
        /// <response code="404">User not found</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ResendEmailVerification(ResendVerificationDto resendVerificationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmailAsync(resendVerificationDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsVerify)
                return BadRequest("Email is already verified");

            var result = await _userService.ResendEmailVerificationAsync(resendVerificationDto.Email);
            if (!result)
                return BadRequest("Failed to send verification email");

            return Ok(new { Message = "Verification email sent. Please check your inbox." });
        }

        /// <summary>
        /// Send verification code to email
        /// </summary>
        /// <param name="resendVerificationCodeDto">Email to send code to</param>
        /// <returns>Send result</returns>
        /// <response code="200">Verification code sent</response>
        /// <response code="400">Invalid email, already verified, or rate limited</response>
        /// <response code="404">User not found</response>
        [HttpPost("send-verification-code")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> SendVerificationCode(ResendVerificationCodeDto resendVerificationCodeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmailAsync(resendVerificationCodeDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsVerify)
                return BadRequest("Email is already verified");

            var result = await _userService.SendVerificationCodeAsync(resendVerificationCodeDto.Email);
            if (!result)
                return BadRequest("Failed to send verification code. Please try again in a few minutes.");

            return Ok(new { Message = "Verification code sent to your email. Please check your inbox." });
        }

        /// <summary>
        /// Resend verification code to email
        /// </summary>
        /// <param name="resendVerificationCodeDto">Email to resend code to</param>
        /// <returns>Resend result</returns>
        /// <response code="200">Verification code sent</response>
        /// <response code="400">Invalid email, already verified, or rate limited</response>
        /// <response code="404">User not found</response>
        [HttpPost("resend-verification-code")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> ResendVerificationCode(ResendVerificationCodeDto resendVerificationCodeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmailAsync(resendVerificationCodeDto.Email);
            if (user == null)
                return NotFound("User not found");

            if (user.IsVerify)
                return BadRequest("Email is already verified");

            var result = await _userService.ResendVerificationCodeAsync(resendVerificationCodeDto.Email);
            if (!result)
                return BadRequest("Failed to send verification code. Please try again in a few minutes.");

            return Ok(new { Message = "Verification code sent to your email. Please check your inbox." });
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token data</param>
        /// <returns>New JWT token and refresh token</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="401">Invalid refresh token</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var user = await _userService.GetUserByRefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            var newToken = _jwtService.GenerateToken(user.UserId, user.Name, user.Email, user.RoleName);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userService.UpdateRefreshTokenAsync(user.UserId, newRefreshToken, refreshTokenExpiry);

            return Ok(new LoginResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                User = user
            });
        }

        /// <summary>
        /// User logout
        /// </summary>
        /// <param name="logoutDto">Logout request containing refresh token</param>
        /// <returns>Logout confirmation</returns>
        /// <response code="200">Logout successful</response>
        [HttpPost("logout")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Logout(LogoutDto logoutDto)
        {
            var user = await _userService.GetUserByRefreshTokenAsync(logoutDto.RefreshToken);
            if (user != null)
            {
                // Clear refresh token
                await _userService.UpdateRefreshTokenAsync(user.UserId, string.Empty, DateTime.UtcNow.AddDays(-1));
            }

            return Ok("Logged out successfully");
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Token validation result</returns>
        /// <response code="200">Token is valid</response>
        /// <response code="400">Token is invalid</response>
        [HttpPost("validate-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult ValidateToken([FromBody] string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return BadRequest("Invalid token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new { 
                IsValid = true,
                UserId = userId,
                Username = username,
                Email = email,
                Role = role
            });
        }
    }

    /// <summary>
    /// Login request - supports both email and phone number authentication
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// User's email address or phone number
        /// Examples: "user@example.com" or "+1234567890" or "1234567890"
        /// </summary>
        /// <example>user@example.com</example>
        [Required(ErrorMessage = "Email or phone number is required")]
        [Display(Name = "Email or Phone")]
        public string EmailOrPhone { get; set; } = string.Empty;
        
        /// <summary>
        /// User's password
        /// </summary>
        /// <example>password123</example>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login response containing authentication tokens and user information
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT access token for API authentication
        /// </summary>
        public string Token { get; set; } = string.Empty;
        
        /// <summary>
        /// Refresh token for obtaining new access tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// Refresh token request
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Valid refresh token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Logout request
    /// </summary>
    public class LogoutDto
    {
        /// <summary>
        /// Refresh token to invalidate
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}