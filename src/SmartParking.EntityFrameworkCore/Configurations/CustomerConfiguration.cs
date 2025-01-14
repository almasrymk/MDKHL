using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.IdentityUser).WithOne().OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.CustomerPackages).WithOne().OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.Invoices).WithOne(_ => _.Customer).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.Payments).WithOne(_ => _.Customers).OnDelete(DeleteBehavior.Restrict);
    }
}
