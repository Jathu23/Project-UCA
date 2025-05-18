using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_UCA.Models;

namespace Project_UCA.Data.EntityConfigurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Unique index
            builder.HasIndex(u => u.EmployeeId).IsUnique();

            // One-to-One relationships
            builder.HasOne(u => u.Address)
                .WithOne(a => a.User)
                .HasForeignKey<Address>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.AccountDetails)
                .WithOne(ad => ad.User)
                .HasForeignKey<AccountDetails>(ad => ad.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.InvoiceData)
                .WithOne(id => id.User)
                .HasForeignKey<InvoiceData>(id => id.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many relationships
            builder.HasMany(u => u.InvoiceHistories)
                .WithOne(ih => ih.User)
                .HasForeignKey(ih => ih.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.UserPermissions)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.ApprovedSignupRequests)
                .WithOne(sr => sr.ApprovedByUser)
                .HasForeignKey(sr => sr.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Many-to-One relationship
            builder.HasOne(u => u.Position)
                .WithMany(p => p.Users)
                .HasForeignKey(u => u.PositionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}