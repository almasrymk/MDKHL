using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SmartParking.Entities.Lookups;
namespace SmartParking.Configurations
{
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        void IEntityTypeConfiguration<Country>.Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x=>x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.HasMany(x => x.Regions).WithOne(x => x.Country).OnDelete(DeleteBehavior.Restrict);
            builder.ToTable("Countries");
        }
    }
    
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        void IEntityTypeConfiguration<City>.Configure(EntityTypeBuilder<City> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.ToTable("Cities");

        }
    }
    public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
    {
        void IEntityTypeConfiguration<Feature>.Configure(EntityTypeBuilder<Feature> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.ArabicDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.Property(x => x.EnglishDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.ToTable("Features");
        }
    }
    public class ScreenConfiguration : IEntityTypeConfiguration<Screen>
    {
        void IEntityTypeConfiguration<Screen>.Configure(EntityTypeBuilder<Screen> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.ArabicDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.Property(x => x.EnglishDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.ToTable("Screens");
        }
    }

    public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
    {
        void IEntityTypeConfiguration<UserType>.Configure(EntityTypeBuilder<UserType> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.ToTable("UserTypes");
        }
    }
    public class PackageTypeConfiguration : IEntityTypeConfiguration<PackageType>
    {
        void IEntityTypeConfiguration<PackageType>.Configure(EntityTypeBuilder<PackageType> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.ArabicDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.Property(x => x.EnglishDescription).HasMaxLength(SmartParkingSettingsConsts.StringMaxLength);
            builder.ToTable("PackageTypes");
        }
    }

    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        void IEntityTypeConfiguration<Region>.Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);

            builder.ToTable("Regions");
        }
    }
    public class StateConfiguration : IEntityTypeConfiguration<State>
    {
        void IEntityTypeConfiguration<State>.Configure(EntityTypeBuilder<State> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);

            builder.ToTable("State");
        }
    }

}
