using Shouldly;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
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
}
