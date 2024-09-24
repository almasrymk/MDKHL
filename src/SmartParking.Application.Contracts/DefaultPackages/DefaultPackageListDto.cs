using Volo.Abp.Application.Dtos;

namespace SmartParking.DefaultPackages
{
    public class DefaultPackageListDto:PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; } 
    }
}
