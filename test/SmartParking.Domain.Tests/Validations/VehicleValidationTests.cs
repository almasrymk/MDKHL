using Shouldly;
using SmartParking.Validations;
using Xunit;

namespace SmartParking.Validations;

public class VehicleValidationTests
{
    [Fact]
    public void Parse_EnglishPlate_ShouldPopulateAllFields()
    {
        // Act
        var result = VehicleValidation.Parse("2252BHR");

        // Assert
        result.ShouldNotBeNull();
        result.EnPlateNumber.ShouldBe("2252");
        result.ArPlateNumber.ShouldBe("٢٢٥٢");
        result.EnPlateLetters.ShouldBe("BHR");
        result.ArPlateLetters.ShouldBe("ر ه ب");
    }

    [Fact]
    public void Parse_ArabicPlate_ShouldPopulateAllFields()
    {
        // Act
        var result = VehicleValidation.Parse("٢٢٥٢رحب");

        // Assert
        result.ShouldNotBeNull();
        result.EnPlateNumber.ShouldBe("2252");
        result.ArPlateNumber.ShouldBe("٢٢٥٢");
        result.ArPlateLetters.ShouldBe("ر ح ب");
        result.EnPlateLetters.ShouldBe("BJR");
    }

    [Fact]
    public void Parse_NullOrEmpty_ShouldReturnEmptyFields()
    {
        var result = VehicleValidation.Parse(string.Empty);
        result.EnPlateNumber.ShouldBe("");
        result.ArPlateNumber.ShouldBe("");
        result.EnPlateLetters.ShouldBe("");
        result.ArPlateLetters.ShouldBe("");
    }

    [Fact]
    public void Parse_MixedScript_ShouldThrowException()
    {
        Should.Throw<System.Exception>(() => VehicleValidation.Parse("2252رحب"));
    }
}
