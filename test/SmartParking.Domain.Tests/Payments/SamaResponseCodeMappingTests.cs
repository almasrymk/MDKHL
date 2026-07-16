using SmartParking.Enums.PosTerminal;
using Shouldly;
using Xunit;

namespace SmartParking.Payments;

public class SamaResponseCodeMappingTests
{
    [Theory]
    [InlineData("000")]
    [InlineData("001")]
    [InlineData("003")]
    [InlineData("007")]
    [InlineData("087")]
    [InlineData("089")]
    [InlineData("400")]
    [InlineData("500")]
    [InlineData("501")]
    public void Evaluate_ShouldReturnApproved_ForApprovedSamaCodes(string code)
    {
        // Act
        var result = SamaResponseCodeMapping.Evaluate(code);

        // Assert
        result.ShouldBe(SamaResponseOutcome.Approved);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("116")]
    [InlineData("117")]
    [InlineData("909")]
    [InlineData("999")]
    [InlineData("")]
    [InlineData(null)]
    public void Evaluate_ShouldReturnDeclined_ForOtherSamaCodes(string code)
    {
        // Act
        var result = SamaResponseCodeMapping.Evaluate(code);

        // Assert
        result.ShouldBe(SamaResponseOutcome.Declined);
    }
}
