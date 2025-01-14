using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.Customer).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.VehicleModel).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.VehicleBrand).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.PlateColor).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.LicenceType).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
