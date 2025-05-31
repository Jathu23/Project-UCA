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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> EmployeeIdExistsAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return false;

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<int> CountMasterUsersAsync()
        {
            var masterRoleId = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Name == "Master")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            return await _context.UserRoles
                .AsNoTracking()
                .CountAsync(ur => ur.RoleId == masterRoleId);
        }

        public async Task<int> CountUsersAsync(string searchTerm, string role, int? positionId)
        {
            var query = BuildUserQuery(searchTerm, role, positionId);
            return await query.CountAsync();
        }

        public async Task<List<ApplicationUser>> SearchUsersAsync(
            string searchTerm, string role, int? positionId,
            string sortBy, bool sortDescending, int skip, int take,
            bool includeAddress, bool includeAccountDetails,
            bool includeInvoiceHistory, bool includeInvoiceData)
        {
            var query = BuildUserQuery(searchTerm, role, positionId);

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // Apply pagination
            query = query.Skip(skip).Take(take);

            // Include related data
            query = IncludeRelatedData(query, includeAddress, includeAccountDetails, includeInvoiceHistory, includeInvoiceData);

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(int userId, bool includeAllDetails)
        {
            var query = _context.Users.AsQueryable();

            if (includeAllDetails)
            {
                query = IncludeRelatedData(query, true, true, true, true);
            }

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<Address> GetAddressByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<AccountDetails> GetAccountDetailsByUserIdAsync(int userId)
        {
            return await _context.AccountDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(ad => ad.UserId == userId);
        }

        public async Task<InvoiceData> GetInvoiceDataByUserIdAndInvoiceNumberAsync(int userId)
        {
            return await _context.InvoiceData
                .AsNoTracking()
                .FirstOrDefaultAsync(id => id.UserId == userId );
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
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAccountDetailsAsync(AccountDetails accountDetails)
        {
            var existing = await _context.AccountDetails.FindAsync(accountDetails.Id);
            if (existing == null)
            {
                throw new ArgumentException($"AccountDetails with ID {accountDetails.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(accountDetails);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            var existing = await _context.Addresses.FindAsync(address.Id);
            if (existing == null)
            {
                throw new ArgumentException($"Address with ID {address.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(address);
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
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserPositionAsync(int userId, int positionId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found.");

            var position = await _context.Positions.FindAsync(positionId);
            if (position == null)
                throw new ArgumentException($"Position with ID {positionId} not found.");

            user.PositionId = positionId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        public async Task<InvoiceData> GetInvoiceDataByUserIdAsync(int userId)
        {
            return await _context.InvoiceData
                .AsNoTracking()
                .FirstOrDefaultAsync(id => id.UserId == userId);
        }

        private IQueryable<ApplicationUser> BuildUserQuery(string searchTerm, string role, int? positionId)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Id.ToString().ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleId = _context.Roles
                    .Where(r => r.Name == role)
                    .Select(r => r.Id)
                    .FirstOrDefault();
                query = query.Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
            }

            if (positionId.HasValue)
            {
                query = query.Where(u => u.PositionId == positionId.Value);
            }

            return query;
        }

        private IQueryable<ApplicationUser> ApplySorting(IQueryable<ApplicationUser> query, string sortBy, bool sortDescending)
        {
            switch (sortBy?.ToLower())
            {
                case "email":
                    return sortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                case "firstname":
                    return sortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                case "createdat":
                    return sortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
                default:
                    return sortDescending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id);
            }
        }

        private IQueryable<ApplicationUser> IncludeRelatedData(IQueryable<ApplicationUser> query,
            bool includeAddress, bool includeAccountDetails,
            bool includeInvoiceHistory, bool includeInvoiceData)
        {
            if (includeAddress)
                query = query.Include(u => u.Address);
            if (includeAccountDetails)
                query = query.Include(u => u.AccountDetails);
            if (includeInvoiceHistory)
                query = query.Include(u => u.InvoiceHistories);
            if (includeInvoiceData)
                query = query.Include(u => u.InvoiceData)
                            .ThenInclude(id => id.InvoiceHistories);

            return query;
        }
    }
}