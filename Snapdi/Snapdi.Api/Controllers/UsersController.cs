using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get current user profile (from JWT token)
        /// </summary>
        [HttpGet("profile")]
        [Authorize] // Any authenticated user can get their own profile
        public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
        {
            try
            {
                // Get user ID from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {   
                    return NotFound(new { error = "User not found", message = $"User with ID {userId} does not exist" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving your profile", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving users", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID (Public endpoint)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = $"User with ID {id} does not exist" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user by email (Admin only)
        /// </summary>
        [HttpGet("by-email")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email required", message = "Email parameter is required" });
                }

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = $"User with email {email} does not exist" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user by phone number (Admin only)
        /// </summary>
        [HttpGet("by-phone")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<UserDto>> GetUserByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    return BadRequest(new { error = "Phone required", message = "Phone parameter is required" });
                }

                var user = await _userService.GetUserByPhoneAsync(phone);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = $"User with phone {phone} does not exist" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Get user with photographer profile (Public endpoint)
        /// </summary>
        [HttpGet("{id}/photographer")]
        public async Task<ActionResult<UserWithPhotographerDto>> GetUserWithPhotographer(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                var user = await _userService.GetUserWithPhotographerProfileAsync(id);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = $"User with ID {id} does not exist or has no photographer profile" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the photographer profile", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a new user (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN")] // Only Admin role can create users
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        error = "Validation failed", 
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Check if email already exists
                if (await _userService.IsEmailExistsAsync(createUserDto.Email))
                {
                    return BadRequest(new { error = "Email already exists", message = $"A user with email {createUserDto.Email} already exists" });
                }

                // Check if phone already exists (if provided)
                if (!string.IsNullOrEmpty(createUserDto.Phone) && await _userService.IsPhoneExistsAsync(createUserDto.Phone))
                {
                    return BadRequest(new { error = "Phone already exists", message = $"A user with phone {createUserDto.Phone} already exists" });
                }

                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while creating the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Update user (User can update own profile, Admin can update any)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // Authenticated users only
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        error = "Validation failed", 
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                // Check if user can update this profile
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    // User can update their own profile OR admin can update any profile
                    if (currentUserId != id && currentUserRole != "Admin")
                    {
                        return Forbid("You can only update your own profile unless you are an admin");
                    }
                }
                else
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", message = $"User with ID {id} does not exist" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can delete users
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { error = "User not found", message = $"User with ID {id} does not exist" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while deleting the user", details = ex.Message });
            }
        }

        /// <summary>
        /// Change user password (User can change own password, Admin can change any)
        /// </summary>
        [HttpPost("{id}/change-password")]
        [Authorize] // Authenticated users only
        public async Task<ActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        error = "Validation failed", 
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                // Check if user can change this password
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    // User can change their own password OR admin can change any password
                    if (currentUserId != id && currentUserRole != "ADMIN")
                    {
                        return Forbid("You can only change your own password unless you are an admin");
                    }
                }
                else
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
                if (!result)
                {
                    return BadRequest(new { error = "Password change failed", message = "Invalid current password or user not found" });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while changing the password", details = ex.Message });
            }
        }

        /// <summary>
        /// Get users by role (Admin only)
        /// </summary>
        [HttpGet("role/{roleId}")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(int roleId)
        {
            try
            {
                if (roleId <= 0)
                {
                    return BadRequest(new { error = "Invalid role ID", message = "Role ID must be a positive number" });
                }

                var users = await _userService.GetUsersByRoleAsync(roleId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving users by role", details = ex.Message });
            }
        }

        /// <summary>
        /// Get active users (Admin only)
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving active users", details = ex.Message });
            }
        }

        /// <summary>
        /// Get verified users (Admin only)
        /// </summary>
        [HttpGet("verified")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can access
        public async Task<ActionResult<IEnumerable<UserDto>>> GetVerifiedUsers()
        {
            try
            {
                var users = await _userService.GetVerifiedUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving verified users", details = ex.Message });
            }
        }

        /// <summary>
        /// Update user status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "ADMIN")] // Only Admin role can update user status
        public async Task<ActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto statusDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                var result = await _userService.UpdateUserStatusAsync(id, statusDto.IsActive, statusDto.IsVerify);
                if (!result)
                {
                    return NotFound(new { error = "User not found", message = $"User with ID {id} does not exist" });
                }

                return Ok(new { message = "User status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating user status", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if email exists (Public endpoint)
        /// </summary>
        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { error = "Email required", message = "Email parameter is required" });
                }

                var exists = await _userService.IsEmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while checking email existence", details = ex.Message });
            }
        }

        /// <summary>
        /// Check if phone number exists (Public endpoint)
        /// </summary>
        [HttpGet("check-phone")]
        public async Task<ActionResult<bool>> CheckPhoneExists([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                {
                    return BadRequest(new { error = "Phone required", message = "Phone parameter is required" });
                }

                var exists = await _userService.IsPhoneExistsAsync(phone);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while checking phone existence", details = ex.Message });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Get current user ID from JWT token claims
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Get current username from JWT token claims
        /// </summary>
        private string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Get current user email from JWT token claims
        /// </summary>
        private string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Get current user role from JWT token claims
        /// </summary>
        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Check if current user is admin
        /// </summary>
        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("ADMIN");
        }

        #endregion
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }
    }
}