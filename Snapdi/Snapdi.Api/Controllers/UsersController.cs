using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

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
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        [HttpGet("by-email")]
        public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get user by phone number
        /// </summary>
        [HttpGet("by-phone")]
        public async Task<ActionResult<UserDto>> GetUserByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest("Phone number is required");

            var user = await _userService.GetUserByPhoneAsync(phone);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get user with photographer profile
        /// </summary>
        [HttpGet("{id}/photographer")]
        public async Task<ActionResult<UserWithPhotographerDto>> GetUserWithPhotographer(int id)
        {
            var user = await _userService.GetUserWithPhotographerProfileAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
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
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ChangePasswordAsync(id, changePasswordDto);
            if (!result)
                return BadRequest("Invalid current password or user not found");

            return Ok("Password changed successfully");
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("role/{roleId}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(int roleId)
        {
            var users = await _userService.GetUsersByRoleAsync(roleId);
            return Ok(users);
        }

        /// <summary>
        /// Get active users
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers()
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get verified users
        /// </summary>
        [HttpGet("verified")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetVerifiedUsers()
        {
            var users = await _userService.GetVerifiedUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Update user status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto statusDto)
        {
            var result = await _userService.UpdateUserStatusAsync(id, statusDto.IsActive, statusDto.IsVerify);
            if (!result)
                return NotFound();

            return Ok("User status updated successfully");
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
        {
            var exists = await _userService.IsEmailExistsAsync(email);
            return Ok(new { exists });
        }

        /// <summary>
        /// Check if phone number exists
        /// </summary>
        [HttpGet("check-phone")]
        public async Task<ActionResult<bool>> CheckPhoneExists([FromQuery] string phone)
        {
            var exists = await _userService.IsPhoneExistsAsync(phone);
            return Ok(new { exists });
        }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }
    }
}