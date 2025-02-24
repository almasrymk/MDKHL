using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Cameras;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Permissions.CameraPermissions;
using System.Globalization;
using System.Security.Authentication;
using Volo.Abp.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using SmartParking.Shifts;
using SmartParking.Blazor.Components.Shared;
using Volo.Abp.Validation;
using SmartParking.Blazor.Components.Pages.Garages;

namespace SmartParking.Blazor.Components.Pages.Shifts;

partial class Shifts
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; 
    public int RowPerPage { get; set; } = 10; 
    public Guid? TenantId { get; private set; }
    private string sortColumn;
    private string ValueOfMenue = "";
    private TableData itemData = new TableData();

    private string termSearch;
    private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };



    protected async override Task OnInitializedAsync()
    {
        if (!await AuthorizationService.IsGrantedAsync(CameraPermissions.Default))
        {

            Navigation.NavigateTo("/Account/AccessDenied", true);
        }
        await CreateMenue();
        await base.OnInitializedAsync();

    }

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

        PagedResultDto<ShiftDto> shiftDto = await ShiftAppService.GetListMdkhlAsync(request, TenantId);

        TotalItem = (int)shiftDto.TotalCount;
        await CreateTable(shiftDto);
    }
    /// <summary>
    /// Fetches the table data for the specified page and updates the table with the retrieved data.
    /// </summary>
    /// <param name="page">The page number for which to fetch the data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
        PagedResultDto<ShiftDto> shiftsDto = new PagedResultDto<ShiftDto>();
        shiftsDto = await ShiftAppService.GetListMdkhlAsync(input, CurrentTenant.Id);
        await CreateTable(shiftsDto);

    }

    private async Task CreateTable(PagedResultDto<ShiftDto> shifts)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["Shifts"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Name"], Name = "Name", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            if (CurrentTenant.Id == null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });

            }
        }
        else
        {

            tableData.Columns.Add(new ColumnDef() { DisplayName = L["Name"], Name = "Name", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = L["GarageName"], Name = "GarageName", IsFilterable = true, PropertyName = "GarageArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            if (CurrentTenant.Id == null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = L["OrganizationName"], Name = "OrganizationName", IsFilterable = true, PropertyName = "OrganizationArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });

            }
        }



        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Description"], Name = "Description", IsFilterable = true, PropertyName = "Description", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartDate"], Name = "StartDate", IsFilterable = true, PropertyName = "StartDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndDate"], Name = "EndDate", IsFilterable = true, PropertyName = "EndDate", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["StartTime"], Name = "StartTime", IsFilterable = true, PropertyName = "StartTime", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["EndTime"], Name = "EndTime", IsFilterable = true, PropertyName = "EndTime", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = L["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });
        foreach (var item in shifts.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Name"], Truncate(item.EnglishName, 10)));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["GarageName"], Truncate(item.GarageEnglishName, 10)));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["OrganizationName"], Truncate(item.OrganizationEnglishName, 10)));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["Name"], Truncate(item.ArabicName, 10)));
                rowDef.Cells.Add(new KeyValuePair<string, string>(L["GarageName"], Truncate(item.GarageArabicName, 10)));
                if (CurrentTenant.Id == null)
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(L["OrganizationName"], Truncate(item.OrganizationArabicName, 10)));
                }
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Description"], Truncate(item.Description, 15)));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartDate"], item.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndDate"], item.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["StartTime"], item.IsFullDay ? 24 + L[$"Hour"] : $" {item.StartTime}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["EndTime"], item.IsFullDay ? 24 + L[$"Hour"] : $" {item.EndTime}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(L["Status"], L[$"{item.Status.ToString()}"]));
            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();
    }

    private Task OpenDialogAsync()
    {

        return CurrentTenant.Id != null ? Dialog.ShowAsync<AddShiftDialog>("Add Shift", options): Dialog.ShowAsync<AddShiftDialogForMDKHL>("Add Shift", options);

    }
    private async Task<IDialogReference> OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var shiftDto = await ShiftAppService.GetShiftAsync(id,CurrentTenant.Id);

        var createUpdateShift = new CreateUpdateShiftDto
        {
            ArabicName = shiftDto.ArabicName,
            EnglishName = shiftDto.EnglishName,
            Status = shiftDto.Status,
            StartDate = shiftDto.StartDate,
            EndDate = shiftDto.EndDate,
            GarageId = shiftDto.GarageId,
            OrganizationId = shiftDto.OrganizationId,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime,
            TenantId = shiftDto.TenantId,
            Description = shiftDto.Description,
            IsFullDay = shiftDto.IsFullDay,
            
        };
        var parameters = new DialogParameters
        {
            ["Shift"] = createUpdateShift,
            ["UpdatedId"] = id
        };


        return await Dialog.ShowAsync<EditShiftDialog>("Edit Shift", parameters);

    }

    private async Task<IDialogReference> OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var shiftDto = await ShiftAppService.GetShiftAsync(id,CurrentTenant.Id);

        var parameters = new DialogParameters
        {
            ["shiftDto"] = shiftDto,
        };


        return await Dialog.ShowAsync<ShiftViewDialog>("CameraView", parameters);
    }


    public string Truncate(string str, int maxLength)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
    }

}
