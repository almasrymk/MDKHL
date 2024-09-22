using SmartParking.Cameras;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Cameras;

public partial class Camera
{
    private List<CameraDto> cameras=new List<CameraDto>();
    private int currentPage = 1;
    private int pageSize = 100;

    private async Task GetData()
    {
        var result = await CamerasAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageSize,
            SkipCount = (currentPage - 1) * pageSize
        });

        // If GetListAsync now returns a plain list, no need for result.Items, just assign directly
        cameras = result.Items.ToList(); // This will now be a List<CameraDto>

        StateHasChanged();
    }
    protected override async Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();
        await GetData();

    }
}
