using Volo.Abp.Application.Dtos;

namespace SmartParking.Packages
{
    public class PackageListDto:PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } 
    }
}
