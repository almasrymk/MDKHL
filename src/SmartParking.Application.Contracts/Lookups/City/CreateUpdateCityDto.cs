namespace SmartParking.Lookups;

public class CreateUpdateCityDto : CreateUpdateBaseDto
{
    public int GovernorateId { get; set; }
    public int RegionId { get; set; }
}
