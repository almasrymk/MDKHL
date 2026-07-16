namespace SmartParking.Organizations;

public class OrganizationSettingsDto
{
    public bool UserAccess { get; set; }
    public bool VehcileAccess { get; set; }
    public bool UseShifts { get; set; }
    public int OwnerVehicles { get; set; }
    public int OwnerSlots { get; set; }
    public int RentalSlots { get; set; }
    public bool IsSlotsEditable { get; set; }
    public bool PosTerminalEnabled { get; set; }
    public string? PosTerminalConnectionMode { get; set; }
    public string? PosTerminalTcpIpAddress { get; set; }
    public int? PosTerminalTcpPort { get; set; }
    public string? PosTerminalComName { get; set; }
    public string? PosTerminalComBaudRate { get; set; }
    public string? PosTerminalComDataBits { get; set; }
    public string? PosTerminalComParity { get; set; }

}
