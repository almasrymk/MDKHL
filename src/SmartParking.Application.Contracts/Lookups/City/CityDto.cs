using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups;
public class CityDto : FullAuditedEntityDto<int>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string StateArabicName { get; set; }
	public string StateEnglishName { get; set; }
    public int StateId { get; set; }
}

