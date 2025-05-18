using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_UCA.Models;

namespace Project_UCA.Data.EntityConfigurations
{
    public class InvoiceDataConfiguration : IEntityTypeConfiguration<InvoiceData>
    {
        public void Configure(EntityTypeBuilder<InvoiceData> builder)
        {
            // Unique index
            builder.HasIndex(id => id.InvoiceNumber).IsUnique();

            // One-to-Many relationship
            builder.HasMany(id => id.InvoiceHistories)
                .WithOne(ih => ih.InvoiceData)
                .HasForeignKey(ih => ih.InvoiceDataId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-One relationship (already configured in ApplicationUser)
        }
    }
}