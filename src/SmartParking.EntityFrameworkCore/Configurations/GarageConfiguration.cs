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

public class GarageConfiguration : IEntityTypeConfiguration<Garage>
{
    public void Configure(EntityTypeBuilder<Garage> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.Organization).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.GarageCategories).WithOne().OnDelete(DeleteBehavior.Restrict);
    }
}
