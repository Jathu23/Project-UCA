using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Project_UCA.DTO;
using Project_UCA.DTOs;
using Project_UCA.Middleware;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_UCA.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionService _permissionService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            IPermissionService permissionService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        public async Task CreateUserAsync(UserCreateDto userDto, int callerUserId)
        {
            if (userDto == null)
                throw new BadRequestException("Invalid user data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to create users.");

            var caller = await _userManager.FindByIdAsync(callerUserId.ToString());
            if (caller == null)
                throw new ArgumentException("Caller user not found.");

            var callerRoles = await _userManager.GetRolesAsync(caller);
            var isMaster = callerRoles.Contains("Master");
            var isAdmin = callerRoles.Contains("Admin");

            if (!new[] { "Master", "Admin", "User" }.Contains(userDto.Role))
                throw new BadRequestException("Invalid role specified.");

            if (userDto.Role == "Master")
            {
                if (!isMaster)
                    throw new UnauthorizedAccessException("Only Master users can create Master users.");
                if (await _userRepository.CountMasterUsersAsync() >= 3)
                    throw new BadRequestException("Maximum 3 Master users allowed.");
            }
            else if (userDto.Role == "Admin" && !isMaster && !isAdmin)
            {
                throw new UnauthorizedAccessException("Only Master or Admin users can create Admin users.");
            }

            if (await _userRepository.EmployeeIdExistsAsync(userDto.EmployeeId))
                throw new BadRequestException("Employee ID already exists.");

            if (await _userRepository.EmailExistsAsync(userDto.Email))
                throw new BadRequestException("Email already exists.");

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
                throw new BadRequestException($"Failed to create user: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, userDto.Role);
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to assign role: {roleErrors}");
            }
        }

        public async Task<List<UserResponseDto>> GetUsersAsync(
            int callerUserId, string searchTerm, string role, int? positionId,
            string sortBy, bool sortDescending, int skip, int take,
            bool includeAddress, bool includeAccountDetails, bool includeInvoiceHistory, bool includeInvoiceData)
        {
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to view users.");

            var users = await _userRepository.SearchUsersAsync(
                searchTerm, role, positionId, sortBy, sortDescending, skip, take,
                includeAddress, includeAccountDetails, includeInvoiceHistory, includeInvoiceData);

            var userDtos = new List<UserResponseDto>();
            foreach (var user in users)
            {
                userDtos.Add(await MapToUserResponseDto(user, includeAddress, includeAccountDetails, includeInvoiceHistory, includeInvoiceData));
            }

            return userDtos;
        }

        public async Task<UserResponseDto> GetUserAsync(int callerUserId, int userId)
        {
            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to view users.");

            var user = await _userRepository.GetUserByIdAsync(userId, true);
            if (user == null)
                throw new ArgumentException("User not found.");

            return await MapToUserResponseDto(user, true, true, true, true);
        }

        public async Task AddAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid account details.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var accountDetails = new AccountDetails
            {
                UserId = dto.UserId,
                BankName = dto.BankName,
                AccountNumber = dto.AccountNumber,
                Branch = dto.Branch,
                SwiftCode = dto.SwiftCode
            };

            await _userRepository.AddAccountDetailsAsync(accountDetails);
        }

        public async Task AddAddressAsync(AddressDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid address data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

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
        }

        public async Task AddInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid invoice data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var invoiceData = new InvoiceData
            {
                UserId = dto.UserId,
                Description = dto.Description,
                Rate = dto.Rate,
                GrossTotal = dto.GrossTotal,
                InvoiceNumber = dto.InvoiceNumber
            };

            await _userRepository.AddInvoiceDataAsync(invoiceData);
        }

        public async Task AddSignatureAsync(IFormFile file, int userId, int callerUserId)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("Invalid file.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var filePath = Path.Combine("Uploads/Signatures", $"{userId}_{DateTime.UtcNow.Ticks}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _userRepository.AddSignatureAsync(userId, filePath);
        }

        public async Task UpdateAccountDetailsAsync(AccountDetailsDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid account details.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var existingAccountDetails = await _userRepository.GetAccountDetailsByUserIdAsync(dto.UserId);
            if (existingAccountDetails == null)
                throw new ArgumentException($"Account details for UserId {dto.UserId} not found.");

            existingAccountDetails.BankName = dto.BankName;
            existingAccountDetails.AccountNumber = dto.AccountNumber;
            existingAccountDetails.Branch = dto.Branch;
            existingAccountDetails.SwiftCode = dto.SwiftCode;

            await _userRepository.UpdateAccountDetailsAsync(existingAccountDetails);
        }

        public async Task UpdateAddressAsync(AddressDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid address data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var existingAddress = await _userRepository.GetAddressByUserIdAsync(dto.UserId);
            if (existingAddress == null)
                throw new ArgumentException($"Address for UserId {dto.UserId} not found.");

            existingAddress.AddressLine1 = dto.AddressLine1;
            existingAddress.AddressLine2 = dto.AddressLine2;
            existingAddress.City = dto.City;
            existingAddress.State = dto.State;
            existingAddress.PostalCode = dto.PostalCode;
            existingAddress.Country = dto.Country;

            await _userRepository.UpdateAddressAsync(existingAddress);
        }

        public async Task UpdateInvoiceDataAsync(InvoiceDataDto_Request dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid invoice data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManageUsers"))
                throw new UnauthorizedAccessException("You do not have permission to manage users.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            var existingInvoiceData = await _userRepository.GetInvoiceDataByUserIdAsync(dto.UserId);
            if (existingInvoiceData == null)
                throw new ArgumentException($"Invoice data for UserId {dto.UserId} not found.");

            existingInvoiceData.Description = dto.Description;
            existingInvoiceData.Rate = dto.Rate;
            existingInvoiceData.GrossTotal = dto.GrossTotal;
            existingInvoiceData.InvoiceNumber = dto.InvoiceNumber;


            await _userRepository.UpdateInvoiceDataAsync(existingInvoiceData);
        }

        public async Task UpdatePositionAsync(UpdateUserPositionDto dto, int callerUserId)
        {
            if (dto == null)
                throw new BadRequestException("Invalid position data.");

            if (!await _permissionService.HasPermissionAsync(callerUserId, "ManagePositions"))
                throw new UnauthorizedAccessException("You do not have permission to manage positions.");

            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                throw new ArgumentException("User not found.");

            await _userRepository.UpdateUserPositionAsync(dto.UserId, dto.PositionId);
        }

        private async Task<UserResponseDto> MapToUserResponseDto(
            ApplicationUser user, bool includeAddress, bool includeAccountDetails,
            bool includeInvoiceHistory, bool includeInvoiceData)
        {
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
                Permissions = permissions
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
                userDto.InvoiceHistory = user.InvoiceHistories
                    .Select(i => new InvoiceHistoryDto
                    {
                        Id = i.Id,
                        UserId = i.UserId,
                        InvoiceDataId = i.InvoiceDataId,
                        Action = i.Action,
                        Timestamp = i.Timestamp,
                        Details = i.Details
                    })
                    .ToList();
            }

            if (includeInvoiceData && user.InvoiceData != null)
            {
                //var latestInvoice = user.InvoiceData
                //    .OrderByDescending(i => i.Id)
                //    .FirstOrDefault();
                //InvoiceHistories = latestInvoice.InvoiceHistories?
                //       .Select(h => new InvoiceHistoryDto
                //       {
                //           Id = h.Id,
                //           UserId = h.UserId,
                //           InvoiceDataId = h.InvoiceDataId,
                //           Action = h.Action,
                //           Timestamp = h.Timestamp,
                //           Details = h.Details
                //       })
                //       .ToList();

                userDto.InvoiceData = new InvoiceDataDto
                {
                    Id = user.InvoiceData.Id,
                    UserId = user.InvoiceData.UserId,
                    Description = user.InvoiceData.Description,
                    Rate = user.InvoiceData.Rate,
                    GrossTotal = user.InvoiceData.GrossTotal,
                    InvoiceNumber = user.InvoiceData.InvoiceNumber,
                   
                };
            }

            return userDto;
        }
    }
}