using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SmartParking.Configurations
{
    public class TenantsConfiguration : IEntityTypeConfiguration<TenantConnectionString>
    {
        void IEntityTypeConfiguration<TenantConnectionString>.Configure(EntityTypeBuilder<TenantConnectionString> builder)
        {
            builder.ConfigureByConvention();
            builder.HasKey(x => x.Id);
           // builder.Property(x => x.TenantId).IsRequired();
            builder.Property(x => x.IpAddress).IsRequired(); 
            builder.Property(x => x.Port).IsRequired();
            builder.Property(x => x.DatabaseName).IsRequired();
            builder.Property(x => x.UserName).IsRequired();
            builder.Property(x => x.Password).IsRequired();


            builder.ToTable("TenantConnectionStrings");
        }
    }
}
