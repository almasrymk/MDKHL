using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Permissions.CameraPermissions;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras;

/// <summary>
/// Component for displaying a list of cameras.
/// </summary>
public partial class Camera
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public Camera(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = CameraPermissions.Create;
        UpdatePolicyName = CameraPermissions.Edit;
        DeletePolicyName = CameraPermissions.Delete;
    }
    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(CameraPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue();
        await base.OnInitializedAsync();
    }
    /// <summary>
    /// filter for cameras on the server side using the camera columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among cameras.</param>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, 
            filters = pageInfo.Filters
        };
        if (CurrentTenant.Id != null)
        {
            var camerassDto = await CamerasAppService.Filter(input);
            TotalItem = (int)camerassDto.TotalCount;
            await CreateTable(camerassDto);
        }
        else
        {
            var camerassDto = await CamerasAppService.FilterForMDKHL(input);
            TotalItem = (int)camerassDto.TotalCount;
            await CreateTable(camerassDto);
        }
    }
    /// <summary>
    /// Creates the menu for the tenants.
    /// </summary>
    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());
        itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, null)) });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.ArabicName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }
        else
        {
            foreach (var org in organizations.Items)
            {
                itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = org.EnglishName, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectTenantAsync(ValueOfMenue, org.TenantId)) });
            }
        }
        MenuDatas = new List<MudMenuData>
        { 
            new MudMenuData
            {
                Name = @L["Organizations"],
            Label = @L[ValueOfMenue],
                Items =itemsOrganizations
            },
        };
    }
    /// <summary>
    /// Handles the selection of a tenant and updates the table data accordingly.
    /// </summary>
    /// <param name="selectedValue"></param>
    /// <param name="tenantId"></param>
    public async Task HandleSelectTenantAsync(string selectedValue, Guid? tenantId)
    {
        TenantId = tenantId;
        ValueOfMenue = selectedValue;
        var request = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn,
            SearchingTerm = termSearch
        };

        PagedResultDto<CameraDto> cameraDto = await CamerasAppService.GetListMdkhlAsync(request,TenantId);

        TotalItem = (int)cameraDto.TotalCount;
        await CreateTable(cameraDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        sortColumn = pageInfo.SortColumn;

        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch
        };
        PagedResultDto<CameraDto> camerassDto = new PagedResultDto<CameraDto>();
        if (CurrentTenant.Id is null)
            camerassDto = await CamerasAppService.GetListMdkhlAsync(input, null);
        else
            camerassDto = await CamerasAppService.GetListOrganizationAsync(input);
        TotalItem = (int)camerassDto.TotalCount;
       await CreateTable(camerassDto);

    }
    /// <summary>
    /// Creates the table and populates it with the cameras list 
    /// </summary>
    /// <param name="cameras"></param>
    private async Task CreateTable(PagedResultDto<CameraDto> cameras)
    {
        var tableData = new TableData()
        {
            Name = L["Cameras"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = true, PropertyName = "DeviceId", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CameraName"], Name = "CameraName", IsFilterable = true, PropertyName = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnGarageName"], Name = "NameEn", IsFilterable = true, PropertyName = "Garage.NameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArGarageName"], Name = "NameAr", IsFilterable = true, PropertyName = "Garage.NameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CameraIP"], Name = "CameraIP", IsFilterable = true, PropertyName = "CameraIP", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["MacAddress"], Name = "MacAddress", IsFilterable = true, PropertyName = "MacAddress", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["CameraType"], Name = "CameraType", IsFilterable = true, PropertyName = "CameraType", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        foreach (var item in cameras.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.DeviceId));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CameraName"], item.Name));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CameraIP"], item.CameraIP));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["MacAddress"], item.MacAddress));
            if (item.Garage is not null)
            {
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnGarageName"], item.Garage.NameEn));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArGarageName"], item.Garage.NameAr));
                }
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["CameraType"], _localizer[$"{item.CameraType.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    /// <summary>
    /// Opens the dialog for adding a new camera.
    /// </summary>
    /// <param name="options"></param>
    private Task OpenDialogAsync(DialogOptions options)//Add Camera
    {
        return Dialog.ShowAsync<AddDialog>("Add Camera", options);
    }
    /// <summary>
    /// Assigns a garage to a camera.
    /// </summary>
    /// <param name="id">The ID of the camera to assign a garage to.</param>
    public async Task<IDialogReference> AssignGarage(string id)
    {
        var Gid = int.Parse(id);
        var cameraDto = await CamerasAppService.GetAsync(Gid);
        var createUpdateCamera  =  _mapper.Map<CreateUpdateCameraDto>(cameraDto);
        var parameters = new DialogParameters
        {
            ["UpdatedId"] = Gid,
            ["Camera"] = createUpdateCamera
        };

        return await Dialog.ShowAsync<AssignGarageDialog>("Assign Garage", parameters);

    }
    /// <summary>
    /// Handles the edit operation for a camera row.
    /// </summary>
    /// <param name="row">The row definition of the camera to edit.</param>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraDto = await CamerasAppService.GetAsync(id);
        var createUpdateCamera = _mapper.Map<CreateUpdateCameraDto>(cameraDto);
        var parameters = new DialogParameters
        {
            ["Camera"] = createUpdateCamera,
            ["UpdatedId"] = id
        };
        return await Dialog.ShowAsync<EditCamera>("Edit Camera", parameters);
    }
    /// <summary>
    /// Handles the click event on a row to view the camera details.
    /// </summary>
    /// <param name="row">The row that was clicked.</param>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var cameraDto = await CamerasAppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["Camera"] = cameraDto,
            ["UpdatedId"] = id

        };
        return await Dialog.ShowAsync<ViewCameraDialog>("CameraView", parameters);
    }
    /// <summary>
    /// Handles the delete operation for a camera row.
    /// </summary>
    /// <param name="row">The row definition of the camera to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name = _localizer["Camera with Mac Address : "] + row.Cells[2].Value;
        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("DeleteCamera", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await CamerasAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{Name} {L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/cameras", forceLoad: true);
            }
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.DELETE]} : {ex.Message} ", Severity.Error);
        }
    }
}
