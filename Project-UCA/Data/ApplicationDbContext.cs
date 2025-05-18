using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project_UCA.Data.EntityConfigurations;
using Project_UCA.Models;

namespace Project_UCA.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Position> Positions { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AccountDetails> AccountDetails { get; set; }
        public DbSet<InvoiceData> InvoiceData { get; set; }
        public DbSet<InvoiceHistory> InvoiceHistories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PositionPermission> PositionPermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<SignupRequest> SignupRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply entity configurations
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new InvoiceDataConfiguration());
            builder.ApplyConfiguration(new AccountDetailsConfiguration());
            builder.ApplyConfiguration(new AddressConfiguration());
           // builder.ApplyConfiguration(new InvoiceHistoryConfiguration());
          //  builder.ApplyConfiguration(new SignupRequestConfiguration());
            builder.ApplyConfiguration(new PositionConfiguration());
            builder.ApplyConfiguration(new PermissionConfiguration());
            builder.ApplyConfiguration(new RolePermissionConfiguration());
            builder.ApplyConfiguration(new PositionPermissionConfiguration());
            builder.ApplyConfiguration(new UserPermissionConfiguration());
        }
    }
}