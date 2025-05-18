using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project_UCA.Data;
using Project_UCA.DTOs;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project_UCA.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            ApplicationDbContext context,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateUserAsync(UserCreateDto userDto, int callerUserId)
        {
            try
            {
                // Get caller user
                var caller = await _userManager.FindByIdAsync(callerUserId.ToString());
                if (caller == null)
                {
                    _logger.LogWarning("User creation failed: Caller user ID {CallerId} not found.", callerUserId);
                    return (false, "Caller user not found.");
                }

                // Check caller permissions and role
                var callerRoles = await _userManager.GetRolesAsync(caller);
                var isMaster = callerRoles.Contains("Master");
                var isAdmin = callerRoles.Contains("Admin");
                var isNormalUserWithPermission = !isMaster && !isAdmin &&
                    await _context.UserPermissions
                        .AnyAsync(up => up.UserId == caller.Id && up.PermissionId == _context.Permissions.First(p => p.Name == "ManageUsers").Id);

                // Validate requested role
                if (!new[] { "Master", "Admin", "User" }.Contains(userDto.Role))
                {
                    _logger.LogWarning("User creation failed: Invalid role {Role}.", userDto.Role);
                    return (false, "Invalid role specified.");
                }

                // Role-based restrictions
                if (userDto.Role == "Master")
                {
                    // Only Master users can create Master users
                    if (!isMaster)
                    {
                        _logger.LogWarning("User creation failed: Caller {CallerId} not authorized to create Master user.", callerUserId);
                        return (false, "Only Master users can create Master users.");
                    }

                    // Check Master user limit (max 3)
                    if (await _userRepository.CountMasterUsersAsync() >= 3)
                    {
                        _logger.LogWarning("User creation failed: Maximum 3 Master users allowed.");
                        return (false, "Maximum 3 Master users allowed.");
                    }
                }
                else if (userDto.Role == "Admin")
                {
                    // Master or Admin can create Admin users
                    if (!isMaster && !isAdmin)
                    {
                        _logger.LogWarning("User creation failed: Caller {CallerId} not authorized to create Admin user.", callerUserId);
                        return (false, "Only Master or Admin users can create Admin users.");
                    }
                }
                else if (userDto.Role == "User")
                {
                    // Master, Admin, or normal user with ManageUsers permission can create User
                    if (!isMaster && !isAdmin && !isNormalUserWithPermission)
                    {
                        _logger.LogWarning("User creation failed: Caller {CallerId} not authorized to create User.", callerUserId);
                        return (false, "You do not have permission to create users.");
                    }
                }

                // Check for duplicate EmployeeId
                if (await _userRepository.EmployeeIdExistsAsync(userDto.EmployeeId))
                {
                    _logger.LogWarning("User creation failed: EmployeeId {EmployeeId} already exists.", userDto.EmployeeId);
                    return (false, "Employee ID already exists.");
                }

                // Check for duplicate Email
                if (await _userRepository.EmailExistsAsync(userDto.Email))
                {
                    _logger.LogWarning("User creation failed: Email {Email} already exists.", userDto.Email);
                    return (false, "Email already exists.");
                }

                // Create ApplicationUser
                var user = new ApplicationUser
                {
                    EmployeeId = userDto.EmployeeId,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    UserName = userDto.Email,
                    PhoneNumber = userDto.PhoneNumber,
                    PositionId = userDto.PositionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create user with password
                var result = await _userManager.CreateAsync(user, userDto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for {Email}: {Errors}", userDto.Email, errors);
                    return (false, $"Failed to create user: {errors}");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, userDto.Role);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Role assignment failed for {Email}: {Errors}", userDto.Email, roleErrors);
                    return (false, $"Failed to assign role: {roleErrors}");
                }

                _logger.LogInformation("User {Email} with role {Role} created successfully by {CallerId}.", userDto.Email, userDto.Role, callerUserId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user {Email} by {CallerId}.", userDto.Email, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }
    }
}