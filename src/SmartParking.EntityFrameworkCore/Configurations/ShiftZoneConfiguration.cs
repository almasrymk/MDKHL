using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Shifts;

namespace SmartParking.Configurations;

public class ShiftZoneConfiguration : IEntityTypeConfiguration<ShiftZone>
{
    public void Configure(EntityTypeBuilder<ShiftZone> builder)
    {
        builder.ComplexProperty(x => x.ShiftManager)
            .IsRequired();
    }
}
