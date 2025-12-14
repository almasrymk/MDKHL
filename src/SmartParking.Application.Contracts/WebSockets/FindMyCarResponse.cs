using System.Text.Json.Serialization;

namespace SmartParking.WebSockets;

public class FindMyCarResponse
{
    [JsonPropertyName("parkingLotId")]
    public string ParkingLotId { get; set; }
    [JsonPropertyName("parkingLotName")]
    public string ParkingLotName { get; set; }
    [JsonPropertyName("parentParkingLotId")]
    public string ParentParkingLotId { get; set; }
    [JsonPropertyName("parentParkingLotName")]
    public string ParentParkingLotName { get; set; }
    [JsonPropertyName("parkingSpaceId")]
    public string ParkingSpaceId { get; set; }
    [JsonPropertyName("parkingSpaceName")]
    public string ParkingSpaceName { get; set; }
    [JsonPropertyName("plateNo")]
    public string PlateNo { get; set; }
    [JsonPropertyName("driveInPicture")]
    public string DriveInPicture { get; set; }
    [JsonPropertyName("plateNoPicture")]
    public string PlateNoPicture { get; set; }
}
