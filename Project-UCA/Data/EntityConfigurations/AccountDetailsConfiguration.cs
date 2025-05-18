using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_UCA.Models;

namespace Project_UCA.Data.EntityConfigurations
{
    public class AccountDetailsConfiguration : IEntityTypeConfiguration<AccountDetails>
    {
        public void Configure(EntityTypeBuilder<AccountDetails> builder)
        {
            // No additional indexes or relationships (already configured in ApplicationUser)
        }
    }
}