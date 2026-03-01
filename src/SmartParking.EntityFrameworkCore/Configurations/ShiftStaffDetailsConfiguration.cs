using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Shifts;

namespace SmartParking.Configurations;


public class ShiftStaffDetailsConfiguration : IEntityTypeConfiguration<ShiftStaffDetails>
{
    public void Configure(EntityTypeBuilder<ShiftStaffDetails> builder)
    {
        builder.HasIndex(e => e.ShiftDetailsId)
             .IsUnique()
             .HasFilter("\"IsDeleted\" = false");
    }
}