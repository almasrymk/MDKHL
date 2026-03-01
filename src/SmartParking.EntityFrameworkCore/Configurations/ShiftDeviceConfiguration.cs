using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Shifts;

namespace SmartParking.Configurations;


public class ShiftDeviceConfiguration : IEntityTypeConfiguration<ShiftDevice>
{
    public void Configure(EntityTypeBuilder<ShiftDevice> builder)
    {
        builder.HasIndex(e => e.ShiftDetailsId)
             .IsUnique()
             .HasFilter("\"IsDeleted\" = false");
    }
}