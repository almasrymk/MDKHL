using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups;

public class CountryDto : FullAuditedEntityDto<int>
{
	public string EnglishName { get; set; }
	public string ArabicName { get; set; }
	public string EnglishCapital { get; set; }
    public string ArabicCapital { get; set; }
}
