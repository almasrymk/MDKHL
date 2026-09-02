using Shouldly;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using System.Linq;
using Xunit;

namespace SmartParking.IntegrationUnits;

public class IntegrationUnitAppServiceTests
{
    [Fact]
    public void CreateUpdateBookingV2Dto_Should_Contain_ProviderName()
    {
        // Arrange & Act
        var dto = new CreateUpdateBookingV2Dto
        {
            UnitId = "U123",
            ProviderName = "ProviderAlpha",
            ProviderArabicName = "مزود ألفا"
        };

        // Assert
        dto.ProviderName.ShouldBe("ProviderAlpha");
        dto.ProviderArabicName.ShouldBe("مزود ألفا");
        dto.UnitId.ShouldBe("U123");
    }

    [Fact]
    public void CreateUpdateBookingV2Dto_InheritsFrom_CreateUpdateBookingDto()
    {
        // Arrange
        var dto = new CreateUpdateBookingV2Dto();

        // Assert
        dto.ShouldBeAssignableTo<CreateUpdateBookingDto>();
    }

    [Fact]
    public void CreateUpdateCustomerIntegrationUnitDto_Cars_Should_Have_MaxLength_3()
    {
        // Arrange
        var propertyInfo = typeof(CreateUpdateCustomerIntegrationUnitDto).GetProperty(nameof(CreateUpdateCustomerIntegrationUnitDto.Cars));
        var maxLengthAttribute = propertyInfo?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), false)
            .Cast<System.ComponentModel.DataAnnotations.MaxLengthAttribute>()
            .FirstOrDefault();

        // Assert
        maxLengthAttribute.ShouldNotBeNull();
        maxLengthAttribute.Length.ShouldBe(3);
    }

    [Fact]
    public void CustomerUnitPackage_Default_IsExternal_Should_Be_False()
    {
        // Arrange & Act
        var entity = new SmartParking.Entities.CustomerUnitPackages.CustomerUnitPackage();

        // Assert
        entity.IsExternal.ShouldBeFalse();
    }

    [Fact]
    public void CustomerUnitPackageAssignationDto_Default_IsExternal_Should_Be_False()
    {
        // Arrange & Act
        var dto = new SmartParking.CustomerUnitPackages.CustomerUnitPackageAssignationDto();

        // Assert
        dto.IsExternal.ShouldBeFalse();
    }

    [Fact]
    public void CreateUpdateCustomerUnitPackageDto_CreateFrom_Should_Propagate_IsExternal_True()
    {
        // Arrange
        var customer = new SmartParking.Cutomers.CustomerDto { Id = System.Guid.NewGuid() };
        var packageDto = new SmartParking.UnitPackages.UnitPackageDto
        {
            Id = 1,
            Package = new SmartParking.Packages.PackageDto
            {
                ArabicName = "باقة",
                EnglishName = "Package",
                Price = 100,
                OrganizationId = 1,
                PublishFrom = new System.TimeOnly(0, 0),
                PublishTo = new System.TimeOnly(0, 0),
                ActivationDate = System.DateTime.UtcNow,
                DectivationDate = System.DateTime.UtcNow.AddDays(3),
                DurationOfPackage = 3,
                CustomerCategory = "Renter",
                PackageCategoryType = SmartParking.Enums.PackageCategoryTypes.CustomPackage
            },
            NumberOfSlots = 1,
            NumberOfVehicles = 1
        };
        var assignDto = new SmartParking.CustomerUnitPackages.CustomerUnitPackageAssignationDto
        {
            UnitCode = "U100",
            DurationOfPackage = 3,
            Discount = 0,
            Vat = 15,
            UnitPackageType = SmartParking.Enums.CustomerUnitPackageType.Renter,
            IsExternal = true
        };

        // Act
        var result = SmartParking.CustomerUnitPackages.CreateUpdateCustomerUnitPackageDto.CreateFrom(customer, packageDto, assignDto);

        // Assert
        result.IsExternal.ShouldBeTrue();
    }

    [Fact]
    public void CreateUpdateCustomerUnitPackageDto_CreateFrom_Should_Propagate_IsExternal_False()
    {
        // Arrange
        var customer = new SmartParking.Cutomers.CustomerDto { Id = System.Guid.NewGuid() };
        var packageDto = new SmartParking.UnitPackages.UnitPackageDto
        {
            Id = 1,
            Package = new SmartParking.Packages.PackageDto
            {
                ArabicName = "باقة",
                EnglishName = "Package",
                Price = 100,
                OrganizationId = 1,
                PublishFrom = new System.TimeOnly(0, 0),
                PublishTo = new System.TimeOnly(0, 0),
                ActivationDate = System.DateTime.UtcNow,
                DectivationDate = System.DateTime.UtcNow.AddDays(3),
                DurationOfPackage = 3,
                CustomerCategory = "Renter",
                PackageCategoryType = SmartParking.Enums.PackageCategoryTypes.CustomPackage
            },
            NumberOfSlots = 1,
            NumberOfVehicles = 1
        };
        var assignDto = new SmartParking.CustomerUnitPackages.CustomerUnitPackageAssignationDto
        {
            UnitCode = "U100",
            DurationOfPackage = 3,
            Discount = 0,
            Vat = 15,
            UnitPackageType = SmartParking.Enums.CustomerUnitPackageType.Renter,
            IsExternal = false
        };

        // Act
        var result = SmartParking.CustomerUnitPackages.CreateUpdateCustomerUnitPackageDto.CreateFrom(customer, packageDto, assignDto);

        // Assert
        result.IsExternal.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CustomerUnitPackageMapper_Should_Map_IsExternal_Correctly(bool isExternal)
    {
        // Arrange
        var entity = new SmartParking.Entities.CustomerUnitPackages.CustomerUnitPackage
        {
            UnitCode = "U100",
            IsExternal = isExternal
        };

        // Act 1: MapToDto
        var dto = SmartParking.Mappers.CustomerUnitPackageMapper.MapToDto(entity);
        dto.IsExternal.ShouldBe(isExternal);

        // Act 2: MapToCreateUpdateDto
        var createUpdateDto = SmartParking.Mappers.CustomerUnitPackageMapper.MapToCreateUpdateDto(entity);
        createUpdateDto.IsExternal.ShouldBe(isExternal);

        // Act 3: ToCreateUpdateDto extension
        var extensionMappedDto = SmartParking.Mappers.CustomerUnitPackageMapper.ToCreateUpdateDto(dto);
        extensionMappedDto.IsExternal.ShouldBe(isExternal);

        // Act 4: UpdateEntityFromDto
        var targetEntity = new SmartParking.Entities.CustomerUnitPackages.CustomerUnitPackage();
        SmartParking.Mappers.CustomerUnitPackageMapper.UpdateEntityFromDto(createUpdateDto, targetEntity);
        targetEntity.IsExternal.ShouldBe(isExternal);
    }
}
