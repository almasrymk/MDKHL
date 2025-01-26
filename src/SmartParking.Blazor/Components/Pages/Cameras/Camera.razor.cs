using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.Permissions.CameraPermissions;
using SmartParking.Permissions.GaragePermissions;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras;

public partial class Camera
{
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10

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

    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(CameraPermissions.Default))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await base.OnInitializedAsync();

    }

    /// <summary>
    /// filter for cameras on the server side using the camera columns.
    /// </summary>
    /// <param name="columnFilter">The name of column to filter for among cameras.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndFilteredResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch, // The term to use for filtering results
            filters = pageInfo.Filters
        };
        // Call the service to perform the search based on the input parameters
        if (CurrentTenant.Id != null)
        {

            var camerassDto = await CamerasAppService.Filter(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerassDto.TotalCount;
            CreateTable(camerassDto);
        }
        else
        {

            var camerassDto = await CamerasAppService.FilterForMDKHL(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerassDto.TotalCount;
            CreateTable(camerassDto);
        }



    }
    /// <summary>
    /// Searches for cameras on the server side using the cameras code search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for among cameras.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        // Log the search term to the console for debugging purposes
        Console.WriteLine(pageInfo.SearchTerm);

        // Update the term used for searching with the provided search term
        termSearch = pageInfo.SearchTerm;

        // Create a new search input object with pagination, sorting, and search term parameters
        var input = new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn,
            SearchingTerm = termSearch // The term to use for filtering results
        };
        if (CurrentTenant.Id != null)
        {

            // Call the service to perform the search based on the input parameters
            var camerasDto = await CamerasAppService.Search(input);

            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerasDto.TotalCount;
            CreateTable(camerasDto);
        }
        else
        {

            var camerassDto = await CamerasAppService.SearchForMDKHL(input);
            // Calculate the total number of pages required to display all results
            TotalItem = (int)camerassDto.TotalCount;
            CreateTable(camerassDto);
        }
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {

        CurrentPage = pageInfo.PageNumber;
        RowPerPage = pageInfo.RowsPerPage;
        Console.WriteLine(RowPerPage);
        // Create a request object for paged and sorted data
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn


        };

        // Retrieve the list of organizations from the server
        var camerassDto = await CamerasAppService.GetListAsync(input);
        TotalItem = (int)camerassDto.TotalCount;
        CreateTable(camerassDto);

    }

    private void CreateTable(PagedResultDto<CameraDto> cameras)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Cameras",
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
    }

    private Task OpenDialogAsync(DialogOptions options)//Add Camera
    {
        return Dialog.ShowAsync<AddDialog>("Add Camera", options);
    }
    public async Task<IDialogReference> AssignGarage(string id)
    {
        var Gid = int.Parse(id);
        var cameraDto = await CamerasAppService.GetAsync(Gid);

        var createUpdateCamera = new CreateUpdateCameraDto
        {
            Name = cameraDto.Name,
            DeviceId = cameraDto.DeviceId,
            Status = cameraDto.Status,
            BrandNameId = cameraDto.Id,
            CameraIP = cameraDto.CameraIP,
            GarageId = cameraDto.GarageId,
            MacAddress = cameraDto.MacAddress,
            MainTypeId = cameraDto.MainType.Id,
            ModelId = cameraDto.Model.Id,
            TenantId = cameraDto.TenantId,
            CameraType = cameraDto.CameraType,
            UserName = cameraDto.UserName,
            Password = cameraDto.Password, 

        };



        var parameters = new DialogParameters
        {
            ["UpdatedId"] = Gid,
            ["Camera"] = createUpdateCamera
        };

        return await Dialog.ShowAsync<AssignGarageDialog>("Assign Garage", parameters);

    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var cameraDto = await CamerasAppService.GetAsync(id);

        var createUpdateCamera = new CreateUpdateCameraDto
        {
            Name = cameraDto.Name,
            DeviceId = cameraDto.DeviceId,
            Status = cameraDto.Status,
            BrandNameId = cameraDto.BrandName.Id,
            CameraIP = cameraDto.CameraIP,
            GarageId = cameraDto.GarageId,
            MacAddress = cameraDto.MacAddress,
            MainTypeId = cameraDto.MainType.Id,
            ModelId = cameraDto.Model.Id,
            TenantId = cameraDto.TenantId,
            UserName = cameraDto.UserName,
            Password = cameraDto.Password,
            CameraType = cameraDto.CameraType

        };



        var parameters = new DialogParameters
        {
            ["Camera"] = createUpdateCamera,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditCamera>("Edit Camera", parameters);

    }

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
