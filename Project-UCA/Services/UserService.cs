using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.DTOs;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;


namespace Project_UCA.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionService _permissionService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            IPermissionService permissionService,
            ApplicationDbContext context,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateUserAsync(UserCreateDto userDto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("User creation failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to create users.");
                }

                var caller = await _userManager.FindByIdAsync(callerUserId.ToString());
                if (caller == null)
                {
                    _logger.LogWarning("User creation failed: Caller user ID {CallerId} not found.", callerUserId);
                    return (false, "Caller user not found.");
                }

                var callerRoles = await _userManager.GetRolesAsync(caller);
                var isMaster = callerRoles.Contains("Master");
                var isAdmin = callerRoles.Contains("Admin");

                if (!new[] { "Master", "Admin", "User" }.Contains(userDto.Role))
                {
                    _logger.LogWarning("User creation failed: Invalid role {Role}.", userDto.Role);
                    return (false, "Invalid role specified.");
                }

                if (userDto.Role == "Master")
                {
                    if (!isMaster)
                    {
                        _logger.LogWarning("User creation failed: Caller {CallerId} not authorized to create Master user.", callerUserId);
                        return (false, "Only Master users can create Master users.");
                    }
                    if (await _userRepository.CountMasterUsersAsync() >= 3)
                    {
                        _logger.LogWarning("User creation failed: Maximum 3 Master users allowed.");
                        return (false, "Maximum 3 Master users allowed.");
                    }
                }
                else if (userDto.Role == "Admin")
                {
                    if (!isMaster && !isAdmin)
                    {
                        _logger.LogWarning("User creation failed: Caller {CallerId} not authorized to create Admin user.", callerUserId);
                        return (false, "Only Master or Admin users can create Admin users.");
                    }
                }

                if (await _userRepository.EmployeeIdExistsAsync(userDto.EmployeeId))
                {
                    _logger.LogWarning("User creation failed: EmployeeId {EmployeeId} already exists.", userDto.EmployeeId);
                    return (false, "Employee ID already exists.");
                }

                if (await _userRepository.EmailExistsAsync(userDto.Email))
                {
                    _logger.LogWarning("User creation failed: Email {Email} already exists.", userDto.Email);
                    return (false, "Email already exists.");
                }

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

                var result = await _userManager.CreateAsync(user, userDto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for {Email}: {Errors}", userDto.Email, errors);
                    return (false, $"Failed to create user: {errors}");
                }

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

        public async Task<(bool Success, List<UserResponseDto> Users, string ErrorMessage)> GetUsersAsync(
            int callerUserId,
            string searchTerm,
            string role,
            int? positionId,
            string sortBy,
            bool sortDescending,
            int skip,
            int take,
            bool includeAddress,
            bool includeAccountDetails,
            bool includeInvoiceHistory,
            bool includeInvoiceData)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Get users failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, null, "You do not have permission to view users.");
                }

                var users = await _userRepository.SearchUsersAsync(
                    searchTerm,
                    role,
                    positionId,
                    sortBy,
                    sortDescending,
                    skip,
                    take,
                    includeAddress,
                    includeAccountDetails,
                    includeInvoiceHistory,
                    includeInvoiceData);

                var userDtos = new List<UserResponseDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    // Fetch permissions for the user
                    var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                    var userDto = new UserResponseDto
                    {
                        Id = user.Id,
                        EmployeeId = user.EmployeeId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        PositionId = user.PositionId,
                        Role = roles.FirstOrDefault(),
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        Permissions = permissions // Include permissions
                    };

                    if (includeAddress && user.Address != null)
                    {
                        userDto.Address = new AddressDto
                        {
                            Id = user.Address.Id,
                            UserId = user.Address.UserId,
                            AddressLine1 = user.Address.AddressLine1,
                            AddressLine2 = user.Address.AddressLine2,
                            City = user.Address.City,
                            State = user.Address.State,
                            PostalCode = user.Address.PostalCode,
                            Country = user.Address.Country
                        };
                    }

                    if (includeAccountDetails && user.AccountDetails != null)
                    {
                        userDto.AccountDetails = new AccountDetailsDto
                        {
                            Id = user.AccountDetails.Id,
                            UserId = user.AccountDetails.UserId,
                            BankName = user.AccountDetails.BankName,
                            AccountNumber = user.AccountDetails.AccountNumber,
                            Branch = user.AccountDetails.Branch,
                            SwiftCode = user.AccountDetails.SwiftCode
                        };
                    }

                    if (includeInvoiceHistory && user.InvoiceHistories != null)
                    {
                        userDto.InvoiceHistory = user.InvoiceHistories.Select(i => new InvoiceHistoryDto
                        {
                            Id = i.Id,
                            UserId = i.UserId,
                            InvoiceDataId = i.InvoiceDataId,
                            Action = i.Action,
                            Timestamp = i.Timestamp,
                            Details = i.Details
                        }).ToList();
                    }

                    if (includeInvoiceData && user.InvoiceData != null)
                    {
                        userDto.InvoiceData = user.InvoiceData != null ? new InvoiceDataDto { Id = user.InvoiceData.Id, UserId = user.InvoiceData.UserId, Description = user.InvoiceData.Description, Rate = user.InvoiceData.Rate, GrossTotal = user.InvoiceData.GrossTotal, InvoiceNumber = user.InvoiceData.InvoiceNumber } : null;
                        
                    }

                    userDtos.Add(userDto);
                }

                _logger.LogInformation("Users retrieved successfully by {CallerId}. Count: {Count}", callerUserId, userDtos.Count);
                return (true, userDtos, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving users by {CallerId}.", callerUserId);
                return (false, null, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, UserResponseDto User, string ErrorMessage)> GetUserAsync(int callerUserId, int userId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Get user failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, null, "You do not have permission to view users.");
                }

                var user = await _userRepository.GetUserByIdAsync(userId, true);
                if (user == null)
                {
                    _logger.LogWarning("Get user failed: User {UserId} not found.", userId);
                    return (false, null, "User not found.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    EmployeeId = user.EmployeeId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    PositionId = user.PositionId,
                    Role = roles.FirstOrDefault(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Permissions = permissions,
                    Address = user.Address != null ? new AddressDto
                    {
                        Id = user.Address.Id,
                        UserId = user.Address.UserId,
                        AddressLine1 = user.Address.AddressLine1,
                        AddressLine2 = user.Address.AddressLine2,
                        City = user.Address.City,
                        State = user.Address.State,
                        PostalCode = user.Address.PostalCode,
                        Country = user.Address.Country
                    } : null,
                    AccountDetails = user.AccountDetails != null ? new AccountDetailsDto
                    {
                        Id = user.AccountDetails.Id,
                        UserId = user.AccountDetails.UserId,
                        BankName = user.AccountDetails.BankName,
                        AccountNumber = user.AccountDetails.AccountNumber,
                        Branch = user.AccountDetails.Branch,
                        SwiftCode = user.AccountDetails.SwiftCode
                    } : null,
                    InvoiceHistory = user.InvoiceHistories?.Select(i => new InvoiceHistoryDto
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        InvoiceDataId = i.InvoiceDataId,
                        Action = i.Action,
                        Timestamp = i.Timestamp,
                        Details = i.Details
                    }).ToList(),
                    InvoiceData = user.InvoiceData != null ? new InvoiceDataDto
                    {
                        Id = user.InvoiceData.Id,
                        UserId = user.InvoiceData.UserId,
                        Description = user.InvoiceData.Description,
                        Rate = user.InvoiceData.Rate,
                        GrossTotal = user.InvoiceData.GrossTotal,
                        InvoiceNumber = user.InvoiceData.InvoiceNumber
                    } : null

                };

                _logger.LogInformation("User {UserId} retrieved successfully by {CallerId}.", userId, callerUserId);
                return (true, userDto, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving user {UserId} by {CallerId}.", userId, callerUserId);
                return (false, null, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Add account details failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Add account details failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var accountDetails = new AccountDetails
                {
                    UserId = dto.UserId,
                    BankName = dto.BankName,
                    AccountNumber = dto.AccountNumber,
                    Branch = dto.Branch,
                    SwiftCode = dto.SwiftCode
                };

                await _userRepository.AddAccountDetailsAsync(accountDetails);
                _logger.LogInformation("Account details added for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Account details added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding account details for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddAddressAsync(AddressDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Add address failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Add address failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var address = new Address
                {
                    UserId = dto.UserId,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    City = dto.City,
                    State = dto.State,
                    PostalCode = dto.PostalCode,
                    Country = dto.Country
                };

                await _userRepository.AddAddressAsync(address);
                _logger.LogInformation("Address added for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Address added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding address for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Add invoice data failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Add invoice data failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var invoiceData = new InvoiceData
                {
                    UserId = dto.UserId,
                    Description = dto.Description,
                    Rate = dto.Rate,
                    GrossTotal = dto.GrossTotal,
                    InvoiceNumber = dto.InvoiceNumber
                };

                await _userRepository.AddInvoiceDataAsync(invoiceData);
                _logger.LogInformation("Invoice data added for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Invoice data added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding invoice data for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> AddSignatureAsync(IFormFile file, int userId, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Add signature failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Add signature failed: User {UserId} not found.", userId);
                    return (false, "User not found.");
                }

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Add signature failed: Invalid file for user {UserId}.", userId);
                    return (false, "Invalid file.");
                }

                var filePath = Path.Combine("Uploads/Signatures", $"{userId}_{DateTime.UtcNow.Ticks}.png");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _userRepository.AddSignatureAsync(userId, filePath);
                _logger.LogInformation("Signature added for user {UserId} by {CallerId}.", userId, callerUserId);
                return (true, "Signature added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding signature for user {UserId} by {CallerId}.", userId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Update account details failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Update account details failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var accountDetails = await _context.AccountDetails.FirstOrDefaultAsync(ad => ad.UserId == dto.UserId);
                if (accountDetails == null)
                {
                    _logger.LogWarning("Update account details failed: No account details found for user {UserId}.", dto.UserId);
                    return (false, "No account details found.");
                }

                accountDetails.BankName = dto.BankName;
                accountDetails.AccountNumber = dto.AccountNumber;
                accountDetails.Branch = dto.Branch;
                accountDetails.SwiftCode = dto.SwiftCode;

                await _userRepository.UpdateAccountDetailsAsync(accountDetails);
                _logger.LogInformation("Account details updated for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Account details updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating account details for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAddressAsync(AddressDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Update address failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Update address failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var address = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == dto.UserId);
                if (address == null)
                {
                    _logger.LogWarning("Update address failed: No address found for user {UserId}.", dto.UserId);
                    return (false, "No address found.");
                }

                address.AddressLine1 = dto.AddressLine1;
                address.AddressLine2 = dto.AddressLine2;
                address.City = dto.City;
                address.State = dto.State;
                address.PostalCode = dto.PostalCode;
                address.Country = dto.Country;

                await _userRepository.UpdateAddressAsync(address);
                _logger.LogInformation("Address updated for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Address updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating address for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId)
        {
            try
            {
                if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                {
                    _logger.LogWarning("Update invoice data failed: Caller {CallerId} lacks ManageUsers permission.", callerUserId);
                    return (false, "You do not have permission to manage users.");
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Update invoice data failed: User {UserId} not found.", dto.UserId);
                    return (false, "User not found.");
                }

                var invoiceData = await _context.InvoiceData.FirstOrDefaultAsync(id => id.UserId == dto.UserId && id.InvoiceNumber == dto.InvoiceNumber);
                if (invoiceData == null)
                {
                    _logger.LogWarning("Update invoice data failed: No invoice data found for user {UserId}.", dto.UserId);
                    return (false, "No invoice data found.");
                }

                invoiceData.Description = dto.Description;
                invoiceData.Rate = dto.Rate;
                invoiceData.GrossTotal = dto.GrossTotal;

                await _userRepository.UpdateInvoiceDataAsync(invoiceData);
                _logger.LogInformation("Invoice data updated for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (true, "Invoice data updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating invoice data for user {UserId} by {CallerId}.", dto.UserId, callerUserId);
                return (false, "An unexpected error occurred.");
            }
        }
    }
}