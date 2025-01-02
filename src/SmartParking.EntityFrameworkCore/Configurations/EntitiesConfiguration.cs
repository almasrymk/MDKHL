using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.TenantManagement;

namespace SmartParking.Configurations;

public static class EntitiesConfiguration
{
    public static void ConfigureEntities(this ModelBuilder builder)
    {
        builder.Entity<Tenant>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => !t.IsDeleted); // Example filter for soft deletion
        });

        builder.Entity<Organization>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => t.Status != Enums.Status.Deleted);
        });

        builder.Entity<Camera>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => t.Status != Enums.Status.Deleted);
        });

        builder.Entity<Garage>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => t.Status != Enums.Status.Deleted);
        });

        builder.Entity<SubscriptionType>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => t.Status != Enums.Status.Deleted);
        });

        builder.Entity<Subscription>(b =>
        {
            // Apply a global query filter to the Tenant entity
            b.HasQueryFilter(t => t.Status != Enums.Status.Deleted);
        });
    }
}
