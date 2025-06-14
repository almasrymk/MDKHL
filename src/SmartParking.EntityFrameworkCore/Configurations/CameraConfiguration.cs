using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SmartParking.Entities.Cameras.Watchers;

namespace SmartParking.Configurations;

public class CameraConfiguration : IEntityTypeConfiguration<Camera>
{
    public void Configure(EntityTypeBuilder<Camera> builder)
    {
        builder.ConfigureByConvention();
        builder.ToTable("Cameras");
        builder.HasOne(_ => _.Model).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.MainType).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.BrandName).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.Garage).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.Organization).WithMany().OnDelete(DeleteBehavior.Restrict);
       
    }
}