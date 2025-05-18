using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_UCA.Models;

namespace Project_UCA.Data.EntityConfigurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // Unique index
            builder.HasIndex(p => p.Name).IsUnique();

            // One-to-Many relationships
            builder.HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PositionPermissions)
                .WithOne(pp => pp.Permission)
                .HasForeignKey(pp => pp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.UserPermissions)
                .WithOne(up => up.Permission)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

        public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
        {
            public void Configure(EntityTypeBuilder<RolePermission> builder)
            {
                // Composite key
                builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            }
        }
    

    public class PositionPermissionConfiguration : IEntityTypeConfiguration<PositionPermission>
        {
            public void Configure(EntityTypeBuilder<PositionPermission> builder)
            {
                // Composite key
                builder.HasKey(pp => new { pp.PositionId, pp.PermissionId });
            }
    }

        public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
        {
            public void Configure(EntityTypeBuilder<UserPermission> builder)
            {
                // Composite key
                builder.HasKey(up => new { up.UserId, up.PermissionId });
            }
        }
    


}