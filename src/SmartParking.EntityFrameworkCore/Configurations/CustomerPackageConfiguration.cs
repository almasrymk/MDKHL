using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Configurations
{
    public class CustomerPackageConfiguration : IEntityTypeConfiguration<CustomerPackage>
    {
        public void Configure(EntityTypeBuilder<CustomerPackage> builder)
        {
            // Configure the relationship between CustomerPackages and Customer
            builder.HasOne(cp => cp.Customer)
                .WithMany(c => c.CustomerPackages) // Assuming Customer entity has ICollection<CustomerPackages>
                .HasForeignKey(cp => cp.CustomerId)
                .IsRequired();

            // Configure the relationship between CustomerPackages and Package
            builder.HasOne(cp => cp.Package)
                .WithMany(p => p.CustomerPackages) // Assuming Package entity has ICollection<CustomerPackages>
                .HasForeignKey(cp => cp.PackageId)
                .IsRequired();

        }
    }
}
