using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class OrganzationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ConfigureByConvention();
        builder.Property(x=>x.Code).HasMaxLength(7);
        builder.HasOne(_ => _.Country).WithMany()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.Region).WithMany(_ => _.Organizations)
            
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.State).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.City).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(_ => _.Subscription).WithMany(_ => _.Organizations).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.OrganizationGarages).WithOne(_ => _.Organization).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.Cameras).WithOne(_ => _.Organization).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.organizationPackages).WithOne(_ => _.Organization).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.Customers).WithOne(_ => _.Organization).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.organizationContactInfos).WithOne().OnDelete(DeleteBehavior.Restrict);
        //builder.HasMany(_ => _.CustomerCategories).WithOne(_ => _.Organization).OnDelete(DeleteBehavior.Restrict);
    }
}
