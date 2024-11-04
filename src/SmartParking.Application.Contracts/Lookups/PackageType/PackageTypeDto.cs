using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups;

public class PackageTypeDto : FullAuditedEntityDto<int>
{
	public string ArabicName { get; set; }
	public string EnglishName { get; set; }
}
