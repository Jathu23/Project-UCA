using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_UCA.Models;

namespace Project_UCA.Data.EntityConfigurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            // One-to-Many relationship
            builder.HasMany(p => p.PositionPermissions)
                .WithOne(pp => pp.Position)
                .HasForeignKey(pp => pp.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many with ApplicationUser (already configured in ApplicationUser)
        }
    }
}