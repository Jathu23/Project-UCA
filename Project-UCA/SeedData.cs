using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;
using Project_UCA.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project_UCA.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context, RoleManager<IdentityRole<int>> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            string[] roleNames = { "Master", "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName, NormalizedName = roleName.ToUpper() });
                }
            }

            // Seed Permissions
            var permissions = new[]
            {
                new Permission { Name = "GenerateInvoice", Description = "Allows generating invoices" },
                new Permission { Name = "EditTemplate", Description = "Allows editing invoice templates" },
                new Permission { Name = "ViewAllInvoices", Description = "Allows viewing all invoices" },
                new Permission { Name = "ManageUsers", Description = "Allows managing users" },
                new Permission { Name = "ManagePermissions", Description = "Allows managing permissions" },
                new Permission { Name = "ManagePositions", Description = "Allows managing positions" }
            };

            foreach (var permission in permissions)
            {
                if (!await context.Permissions.AnyAsync(p => p.Name == permission.Name))
                {
                    context.Permissions.Add(permission);
                }
            }
            await context.SaveChangesAsync();

            // Seed RolePermissions
            var masterRole = await roleManager.FindByNameAsync("Master");
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var userRole = await roleManager.FindByNameAsync("User");

            var rolePermissions = new[]
            {
                // Master Role
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "GenerateInvoice").Id },
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "EditTemplate").Id },
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "ViewAllInvoices").Id },
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "ManageUsers").Id },
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "ManagePermissions").Id },
                new RolePermission { RoleId = masterRole.Id, PermissionId = context.Permissions.First(p => p.Name == "ManagePositions").Id },
                // Admin Role
                new RolePermission { RoleId = adminRole.Id, PermissionId = context.Permissions.First(p => p.Name == "GenerateInvoice").Id },
                new RolePermission { RoleId = adminRole.Id, PermissionId = context.Permissions.First(p => p.Name == "EditTemplate").Id },
                new RolePermission { RoleId = adminRole.Id, PermissionId = context.Permissions.First(p => p.Name == "ViewAllInvoices").Id },
                // User Role
                new RolePermission { RoleId = userRole.Id, PermissionId = context.Permissions.First(p => p.Name == "GenerateInvoice").Id }
            };

            foreach (var rp in rolePermissions)
            {
                if (!await context.RolePermissions.AnyAsync(x => x.RoleId == rp.RoleId && x.PermissionId == rp.PermissionId))
                {
                    context.RolePermissions.Add(rp);
                }
            }
            await context.SaveChangesAsync();

            // Seed Positions
            var position = new Position
            {
                Name = "Manager",
                Description = "Default managerial position",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (!await context.Positions.AnyAsync(p => p.Name == position.Name))
            {
                context.Positions.Add(position);
            }
            await context.SaveChangesAsync();

            // Seed PositionPermissions
            var managerPosition = await context.Positions.FirstAsync(p => p.Name == "Manager");
            var positionPermissions = new[]
            {
                new PositionPermission { PositionId = managerPosition.Id, PermissionId = context.Permissions.First(p => p.Name == "GenerateInvoice").Id },
                new PositionPermission { PositionId = managerPosition.Id, PermissionId = context.Permissions.First(p => p.Name == "ViewAllInvoices").Id }
            };

            foreach (var pp in positionPermissions)
            {
                if (!await context.PositionPermissions.AnyAsync(x => x.PositionId == pp.PositionId && x.PermissionId == pp.PermissionId))
                {
                    context.PositionPermissions.Add(pp);
                }
            }
            await context.SaveChangesAsync();

            // Seed Master User
            var masterUserEmail = "master@example.com";
            if (!await userManager.Users.AnyAsync(u => u.Email == masterUserEmail))
            {
                var masterUser = new ApplicationUser
                {
                    EmployeeId = "EMP002",
                    FirstName = "Master",
                    LastName = "User",
                    Email = masterUserEmail,
                    UserName = masterUserEmail,
                    PhoneNumber = "+1234567890",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // Skip email confirmation
                };

                var result = await userManager.CreateAsync(masterUser, "Master@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(masterUser, "Master");
                }
                else
                {
                    throw new Exception($"Failed to create Master user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}