using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Organizations;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class UnitPackageConfiguration : IEntityTypeConfiguration<UnitPackage>
{
    public void Configure(EntityTypeBuilder<UnitPackage> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.Package)
            .WithOne()
            .HasForeignKey<UnitPackage>(_ => _.PackageId);
    }
}
