
using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups.Country;

public class CountryLookupDto : FullAuditedEntityDto<int>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ArabicCapital { get; set; }
    public string EnglishCapital { get; set; }
}
