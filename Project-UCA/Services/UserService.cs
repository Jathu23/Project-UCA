using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Project_UCA.DTOs;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Project_UCA.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateUserAsync(UserCreateDto userDto)
        {
            try
            {
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
                    UserName = userDto.Email, // UserName same as Email
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

                // Assign default "User" role
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Role assignment failed for {Email}: {Errors}", userDto.Email, roleErrors);
                    return (false, $"Failed to assign role: {roleErrors}");
                }

                _logger.LogInformation("User {Email} created successfully.", userDto.Email);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user {Email}.", userDto.Email);
                return (false, "An unexpected error occurred.");
            }
        }
    }
}