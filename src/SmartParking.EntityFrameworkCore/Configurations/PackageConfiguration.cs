using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.PackageType).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.Organization).WithMany(_ => _.organizationPackages).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.CustomerPackages).WithOne(_=> _.Package).OnDelete(DeleteBehavior.Restrict);
        
    }
}
