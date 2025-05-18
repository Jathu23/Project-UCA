using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project_UCA.Data;
using Project_UCA.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project_UCA.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<List<Claim>> GetUserPermissionClaims(ApplicationUser user)
        {
            var claims = new List<Claim>();

            // Role-based permissions
            var rolePermissions = await _dbContext.RolePermissions
                .Where(rp => _dbContext.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == rp.RoleId))
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            // Position-based permissions
            var positionPermissions = user.PositionId.HasValue
                ? await _dbContext.PositionPermissions
                    .Where(pp => pp.PositionId == user.PositionId.Value)
                    .Select(pp => pp.Permission.Name)
                    .ToListAsync()
                : new List<string>();

            // User-specific permissions
            var userPermissions = await _dbContext.UserPermissions
                .Where(up => up.UserId == user.Id)
                .Select(up => up.Permission.Name)
                .ToListAsync();

            var allPermissions = rolePermissions.Union(positionPermissions).Union(userPermissions);
            claims.AddRange(allPermissions.Select(p => new Claim("Permission", p)));

            return claims;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add permission claims
            var permissionClaims = await GetUserPermissionClaims(user);
            claims.AddRange(permissionClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
