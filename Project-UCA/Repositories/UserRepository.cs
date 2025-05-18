using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Models;
using Project_UCA.Repositories.Interfaces;
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
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}