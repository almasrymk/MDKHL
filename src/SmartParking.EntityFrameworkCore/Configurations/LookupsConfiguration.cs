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

            builder.ToTable("Countries");
        }
    }
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        void IEntityTypeConfiguration<Governorate>.Configure(EntityTypeBuilder<Governorate> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.ToTable("Governorates");
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

    public class EmaraaConfiguration : IEntityTypeConfiguration<Emaraa>
    {
        void IEntityTypeConfiguration<Emaraa>.Configure(EntityTypeBuilder<Emaraa> builder)
        {
            builder.ConfigureByConvention();
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ArabicName).IsRequired();
            builder.Property(x => x.ArabicName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);
            builder.Property(x => x.EnglishName).HasMaxLength(SmartParkingSettingsConsts.StringMinLength);

            builder.ToTable("Emaraas");
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
