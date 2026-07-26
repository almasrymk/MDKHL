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
    public void Evaluate_ShouldReturnApproved_ForApprovedSamaCodes(string code)
    {
        // Act
        var result = SamaResponseCodeMapping.Evaluate(code);

        // Assert
        result.ShouldBe(SamaResponseOutcome.Approved);
    }

    [Fact]
    public void Evaluate_ShouldReturnReconciliationSuccessful_ForCode500()
    {
        SamaResponseCodeMapping.Evaluate("500").ShouldBe(SamaResponseOutcome.ReconciliationSuccessful);
    }

    [Fact]
    public void Evaluate_ShouldReturnReconciliationFailed_ForCode501()
    {
        SamaResponseCodeMapping.Evaluate("501").ShouldBe(SamaResponseOutcome.ReconciliationFailed);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("116")]
    [InlineData("117")]
    [InlineData("909")]
    [InlineData("999")]
    [InlineData("")]
    [InlineData(null)]
    public void Evaluate_ShouldReturnDeclined_ForOtherSamaCodes(string? code)
    {
        // Act
        var result = SamaResponseCodeMapping.Evaluate(code);

        // Assert
        result.ShouldBe(SamaResponseOutcome.Declined);
    }
}
