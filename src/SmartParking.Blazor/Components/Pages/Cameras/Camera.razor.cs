using MudBlazor;
using SmartParking.Cameras;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using static MudBlazor.CategoryTypes;

namespace SmartParking.Blazor.Components.Pages.Cameras;

public partial class Camera
{
    private List<CameraDto> cameras=new List<CameraDto>();
    private int currentPage = 1;
    private int pageSize = 100;
    //Dialog
    private readonly DialogOptions _maxWidth = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };
    private readonly DialogOptions _closeButton = new() { CloseButton = true };
    private readonly DialogOptions _noHeader = new() { NoHeader = true };
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly DialogOptions _fullScreen = new() { FullScreen = true, CloseButton = true };
    private readonly DialogOptions _topCenter = new() { Position = DialogPosition.TopCenter };


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

    private async Task OpenEditDialog(CameraDto camera)
    {
        var parameters = new DialogParameters { ["Camera"] = camera };
        var dialog = Dialog.Show<EditCamera>("Edit Camera", parameters);
        var result = await dialog.Result;

       
    }
    private Task OpenDialogAsync(DialogOptions options)
    {
        return Dialog.ShowAsync<AddDialog>("Add Camera", options);
    }
}
