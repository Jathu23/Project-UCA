using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_UCA.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EmployeeIdExistsAsync(string employeeId)
        {
            return await _context.Users.AnyAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<int> CountMasterUsersAsync()
        {
            return await _context.UserRoles
                .CountAsync(ur => ur.RoleId == _context.Roles.First(r => r.Name == "Master").Id);
        }

        public async Task<int> CountUsersAsync(string searchTerm, string role, int? positionId)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.First(r => r.Name == role).Id));
            }

            if (positionId.HasValue)
            {
                query = query.Where(u => u.PositionId == positionId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<List<ApplicationUser>> SearchUsersAsync(
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
            var query = _context.Users.AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.First(r => r.Name == role).Id));
            }

            if (positionId.HasValue)
            {
                query = query.Where(u => u.PositionId == positionId.Value);
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "email":
                    query = sortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                    break;
                case "firstname":
                    query = sortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                    break;
                case "createdat":
                    query = sortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                    break;
                default:
                    query = sortDescending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id);
                    break;
            }

            // Apply pagination
            query = query.Skip(skip).Take(take);

            // Include related data
            if (includeAddress)
            {
                query = query.Include(u => u.Address);
            }
            if (includeAccountDetails)
            {
                query = query.Include(u => u.AccountDetails);
            }
            if (includeInvoiceHistory)
            {
                query = query.Include(u => u.InvoiceHistories);
            }
            if (includeInvoiceData)
            {
                query = query.Include(u => u.InvoiceData).ThenInclude(id => id.InvoiceHistories);
            }

            return await query.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(int userId, bool includeAllDetails)
        {
            var query = _context.Users.AsQueryable();
            if (includeAllDetails)
            {
                query = query
                    .Include(u => u.Address)
                    .Include(u => u.AccountDetails)
                    .Include(u => u.InvoiceHistories)
                    .Include(u => u.InvoiceData).ThenInclude(id => id.InvoiceHistories);
            }
            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task AddAccountDetailsAsync(AccountDetails accountDetails)
        {
            await _context.AccountDetails.AddAsync(accountDetails);
            await _context.SaveChangesAsync();
        }

        public async Task AddAddressAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
        }

        public async Task AddInvoiceDataAsync(InvoiceData invoiceData)
        {
            await _context.InvoiceData.AddAsync(invoiceData);
            await _context.SaveChangesAsync();
        }

        public async Task AddSignatureAsync(int userId, string signaturePath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.");
            }

            user.Signature = signaturePath;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAccountDetailsAsync(AccountDetails accountDetails)
        {
            var existing = await _context.AccountDetails.FindAsync(accountDetails.Id);
            if (existing == null)
            {
                throw new ArgumentException($"AccountDetails with ID {accountDetails.Id} not found.");
            }

            existing.BankName = accountDetails.BankName;
            existing.AccountNumber = accountDetails.AccountNumber;
            existing.Branch = accountDetails.Branch;
            existing.SwiftCode = accountDetails.SwiftCode;

            _context.AccountDetails.Update(existing);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            var existing = await _context.Addresses.FindAsync(address.Id);
            if (existing == null)
            {
                throw new ArgumentException($"Address with ID {address.Id} not found.");
            }

            existing.AddressLine1 = address.AddressLine1;
            existing.AddressLine2 = address.AddressLine2;
            existing.City = address.City;
            existing.State = address.State;
            existing.PostalCode = address.PostalCode;
            existing.Country = address.Country;

            _context.Addresses.Update(existing);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInvoiceDataAsync(InvoiceData invoiceData)
        {
            var existing = await _context.InvoiceData
                .FirstOrDefaultAsync(id => id.UserId == invoiceData.UserId && id.InvoiceNumber == invoiceData.InvoiceNumber);
            if (existing == null)
            {
                throw new ArgumentException($"InvoiceData with UserId {invoiceData.UserId} and InvoiceNumber {invoiceData.InvoiceNumber} not found.");
            }

            existing.Description = invoiceData.Description;
            existing.Rate = invoiceData.Rate;
            existing.GrossTotal = invoiceData.GrossTotal;

            _context.InvoiceData.Update(existing);
            await _context.SaveChangesAsync();
        }
    }
}