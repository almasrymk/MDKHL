using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Localization;
using SmartParking.Permissions.CameraPermissions;
using SmartParking.Permissions.DispenserPermissions;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras.Dispensers;

public partial class Dispenser
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

    public Dispenser(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = DispenserPermissions.Create;
        UpdatePolicyName = DispenserPermissions.Edit;
        DeletePolicyName = DispenserPermissions.Delete;
    }
    /// <summary>
    /// Called when the component is initialized.
    /// </summary>
    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(DispenserPermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue();
        await base.OnInitializedAsync();
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

        PagedResultDto<DispenserDto> dispenserDto = await DispenserAppService.GetAllAsync(request, TenantId);

        TotalItem = (int)dispenserDto.TotalCount;
        await CreateTable(dispenserDto);
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
        PagedResultDto<DispenserDto> dispenserDto = new PagedResultDto<DispenserDto>();
        if (CurrentTenant.Id is null)
            dispenserDto = await DispenserAppService.GetAllAsync(input, null);
        else
            dispenserDto = await DispenserAppService.GetAllAsync(input, CurrentTenant.Id.Value);
        TotalItem = (int)dispenserDto.TotalCount;
        await CreateTable(dispenserDto);

    }
    /// <summary>
    /// Creates the table and populates it with the cameras list 
    /// </summary>
    /// <param name="cameras"></param>
    private async Task CreateTable(PagedResultDto<DispenserDto> cameras)
    {
        var tableData = new TableData()
        {
            Name = L["Dispenser"],
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Code"], Name = "Code", IsFilterable = true, PropertyName = "DeviceId", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Name"], Name = "Name", IsFilterable = true, PropertyName = "Name", IsSortable = true, TypeFilter = TypeFilter.Text });

        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Type"], Name = "Type", IsFilterable = true, PropertyName = "CameraType", IsSortable = true, TypeFilter = TypeFilter.Enum });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        foreach (var item in cameras.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Code"], item.DeviceId));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Name"], item.Name));
            
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Type"], _localizer[$"{item.CameraType.ToString()}"]));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Status.ToString()}"]));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }
    /// <summary>
    /// Opens the dialog for adding a new dispenser.
    /// </summary>
    /// <param name="options"></param>
    private Task OpenDialogAsync(DialogOptions options)
    {
        return Dialog.ShowAsync<AddDispenser>("Add Dispenser", options);
    }
    /// <summary>
    /// Handles the edit operation for a dispenser row.
    /// </summary>
    /// <param name="row">The row definition of the dispenser to edit.</param>
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var dispenserDto = await DispenserAppService.GetAsync(id);
        var createUpdateDispenser = _mapper.Map<CreateUpdateDispenserDto>(dispenserDto);
        var parameters = new DialogParameters
        {
            ["Dispenser"] = createUpdateDispenser,
            ["UpdatedId"] = id
        };
        return await Dialog.ShowAsync<EditDispenser>("Edit Dispenser", parameters);
    }
    /// <summary>
    /// Handles the click event on a row to view the dispenser details.
    /// </summary>
    /// <param name="row">The row that was clicked.</param>
    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var dispenserDto = await DispenserAppService.GetAsync(id);
        var parameters = new DialogParameters
        {
            ["Dispenser"] = dispenserDto,
            ["UpdatedId"] = id

        };
        return await Dialog.ShowAsync<ViewDispenser>("Dispenser View", parameters);
    }
    /// <summary>
    /// Handles the delete operation for a dispenser row.
    /// </summary>
    /// <param name="row">The row definition of the dispenser to delete.</param>
    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var Name =  row.Cells[2].Value;
        var parameters = new DialogParameters
        {
            ["Name"] = Name,
        };
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("Delete Dispenser", parameters);
        var result = await dialog.Result;
        try
        {
            if (!result.Canceled)
            {
                await DispenserAppService.DeleteAsync(id);
                SnackBar.ShowSnackbar(Snackbar, $"{Name} {L[AlertMessages.AlertMessages.DELETE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/dispensers", forceLoad: true);
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
