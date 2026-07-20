using SmartParking.Organizations;
using Xunit;

namespace SmartParking.Payments;

public class PosTerminalSettingsSectionBindingTests
{
    [Fact]
    public void Settings_PropertyMutation_PropagatesToOriginalReference()
    {
        // Arrange
        var parentDto = new OrganizationSettingsDto
        {
            PosTerminalEnabled = false,
            PosTerminalConnectionMode = "TCP",
            PosTerminalTcpIpAddress = "127.0.0.1",
            PosTerminalTcpPort = 5000
        };

        // Simulate child component receiving parameter by reference
        OrganizationSettingsDto childSettingsParam = parentDto;

        // Act: Simulate child component mutating properties via @bind-Value setter callbacks
        childSettingsParam.PosTerminalEnabled = true;
        childSettingsParam.PosTerminalConnectionMode = "COM";
        childSettingsParam.PosTerminalComName = "COM3";
        childSettingsParam.PosTerminalTcpIpAddress = "192.168.1.100";

        // Assert: Parent reference reflects mutations directly
        Assert.True(parentDto.PosTerminalEnabled);
        Assert.Equal("COM", parentDto.PosTerminalConnectionMode);
        Assert.Equal("COM3", parentDto.PosTerminalComName);
        Assert.Equal("192.168.1.100", parentDto.PosTerminalTcpIpAddress);
    }
}
