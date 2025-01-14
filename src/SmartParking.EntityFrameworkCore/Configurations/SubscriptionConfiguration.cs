using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ConfigureByConvention();
        builder.HasOne(_ => _.SubscriptionType).WithMany(_ => _.Subscriptions).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(_ => _.Organizations).WithOne(_ => _.Subscription).OnDelete(DeleteBehavior.Restrict);
    }
}
