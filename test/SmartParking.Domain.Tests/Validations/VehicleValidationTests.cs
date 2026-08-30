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

    [Theory]
    [InlineData("2252BHR", true)]
    [InlineData("٢٢٥٢رحب", true)]
    [InlineData("1234 BHR", true)]
    [InlineData("BAP1324", false)] // 'P' is not a valid Saudi character
    [InlineData("12345BHR", false)] // >4 digits
    [InlineData("ABCD1234", false)] // >3 letters
    [InlineData("1234", false)] // Missing letters
    [InlineData("BHR", false)] // Missing digits
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidSaudiPlate_Should_Validate_Correctly(string? plate, bool expected)
    {
        var result = VehicleValidation.IsValidSaudiPlate(plate!);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("MAI12345678", true)]
    [InlineData("DUR51975320", true)]
    [InlineData("1324678901", true)]
    [InlineData("74108520963", true)]
    [InlineData("MA12345678", false)] // 2 letters + 8 digits
    [InlineData("MAI1234567", false)] // 3 letters + 7 digits
    [InlineData("MAI123456789", false)] // 3 letters + 9 digits
    [InlineData("123456789", false)] // 9 digits
    [InlineData("123456789012", false)] // 12 digits
    [InlineData("ABCD12345678", false)] // 4 letters + 8 digits
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidTicketNumber_Should_Validate_Correctly(string? ticket, bool expected)
    {
        var result = VehicleValidation.IsValidTicketNumber(ticket!);
        result.ShouldBe(expected);
    }
}

