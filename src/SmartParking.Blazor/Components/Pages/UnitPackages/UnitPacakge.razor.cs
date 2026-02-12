using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Blazor.Components.Pages.Packages;
using SmartParking.Blazor.Components.SPComponents;
using SmartParking.Currency;
using SmartParking.DefaultPackages;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Packages;
using SmartParking.Permissions.PackagePermissions;
using SmartParking.UnitPackages;
using System.Globalization;
using Volo.Abp.Application.Dtos;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SmartParking.Blazor.Components.Pages.UnitPackages;

public partial class UnitPacakge
{
    private List<MudMenuData> MenuDatas = new List<MudMenuData>();
    private string sortColumn;
    private Guid? TenantId;
    private int? OrganizationId;
    private string ValueOfMenue { get; set; } = "All";

    private int TotalItem;
    public int CurrentPage { get; set; } = 1;
    public int RowPerPage { get; set; } = 10;

    private TableData itemData = new TableData();
    private string termSearch;
    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    public UnitPacakge(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;
        CreatePolicyName = PackagePermissions.Create;
        UpdatePolicyName = PackagePermissions.Edit;
        DeletePolicyName = PackagePermissions.Delete;
    }

    protected async override Task OnInitializedAsync()
    {
        await CreateMenue();
        // Permission check for the page
        if (!await AuthorizationService.IsGrantedAsync(PackagePermissions.Default))
        {
            Navigation.NavigateTo("/Account/AccessDenied", true);
        }

        await base.OnInitializedAsync();
    }

    private async Task HandleFilter(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        // Note: Adaptation required if your AppService has a specific Filter method
        var result = await UnitPackageAppService.GetListAsync(input);
        TotalItem = (int)result.TotalCount;
        await CreateTable(result);
    }

    private async Task CreateMenue()
    {
        var itemsOrganizations = new List<MenuItem>();
        var organizations = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });

        itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = @L["All"], OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectOrganizationAsync("All", null)) });

        foreach (var org in organizations.Items)
        {
            string label = CultureInfo.CurrentCulture.Name == "ar" ? org.ArabicName : org.EnglishName;
            itemsOrganizations.Add(new MenuItem { MenueName = "Organization", Label = label, OnClick = EventCallback.Factory.Create<string>(this, async (string selectedValue) => await HandleSelectOrganizationAsync(label, org.Id)) });
        }

        MenuDatas = new List<MudMenuData>
        {
            new MudMenuData { Name = @L["Organizations"], Label = @L[ValueOfMenue], Items = itemsOrganizations },
        };
    }

    public async Task HandleSelectOrganizationAsync(string selectedValue, int? orgId)
    {
        OrganizationId = orgId;
        ValueOfMenue = selectedValue;

        var request = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = RowPerPage,
            SkipCount = (CurrentPage - 1) * RowPerPage,
            Sorting = sortColumn
        };

        PagedResultDto<UnitPackageDto> result;
        if (OrganizationId.HasValue)
        {
            result = await UnitPackageAppService.GetUnitPackagesByOrganization(request, OrganizationId.Value);
        }
        else
        {
            result = await UnitPackageAppService.GetListAsync(request);
        }

        TotalItem = (int)result.TotalCount;
        await CreateTable(result);
    }

    private async Task HandleSearch(PageChangeEventArgs pageInfo)
    {
        termSearch = pageInfo.SearchTerm;
        var input = new PagedAndSortedResultRequestDto // Use your search DTO if applicable
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var result = await UnitPackageAppService.GetListAsync(input);
        TotalItem = (int)result.TotalCount;
        await CreateTable(result);
    }

    private async Task FetchTableData(PageChangeEventArgs pageInfo)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageInfo.RowsPerPage,
            SkipCount = (pageInfo.PageNumber - 1) * pageInfo.RowsPerPage,
            Sorting = pageInfo.SortColumn
        };

        var result = await UnitPackageAppService.GetListAsync(input);
        TotalItem = (int)result.TotalCount;
        await CreateTable(result);
    }

    private async Task CreateTable(PagedResultDto<UnitPackageDto> packages)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = L["Packages"],
            ShowHeader = true
        };


        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Id"], Name = "Id", IsFilterable = false, PropertyName = "Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["EnglishName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["ArabicName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{_localizer["Price"]}", Name = "Price", IsFilterable = false, PropertyName = "Price", IsSortable = true, TypeFilter = TypeFilter.Text, TypeOfColumn = typeof(decimal) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Published"], Name = "published", IsFilterable = false, PropertyName = "published", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CurrentTenant.Id == null)
        {
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "EnglishName", IsFilterable = true, PropertyName = "EnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["OrganizationName"], Name = "ArabicName", IsFilterable = true, PropertyName = "ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            }

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = _localizer["Status"], Name = "Status", IsFilterable = false, PropertyName = "Status", IsSortable = true, TypeFilter = TypeFilter.Enum });

        //tableData.Columns.Add(new ColumnDef() { DisplayName = "Max Capacity", Name = "MaxCapacity", IsFilterable = false, IsSortable = true, TypeFilter = TypeFilter.Number });
        foreach (var item in packages.Items)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(nameof(item.Id), item.Id.ToString()));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["EnglishName"], item.Package.EnglishName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["ArabicName"], item.Package.ArabicName));
            }
            var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{_localizer["Price"]}",
                $"{item.Package.Price} {IconHtml}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Published"], item.Package.Published ? 24 + _localizer[$"Hour"] : $" {item.Package.PublishFrom} " + " - " + $" {item.Package.PublishTo}"));
            if (CurrentTenant.Id == null)
            {
                if (CultureInfo.CurrentCulture.Name == "en")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.Package.OrganizationEnglishName));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["OrganizationName"], item.Package.OrganizationArabicName));
                }

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(_localizer["Status"], _localizer[$"{item.Package.Status.ToString()}"]));


            tableData.Rows.Add(rowDef);
        }
        itemData = tableData;
        await Task.Yield();

    }


    private async Task OpenAddDialogAsync(DialogOptions options)
    {
        var dialog = await Dialog.ShowAsync<AddUnitPackageDialog>("Add Unit Package", options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await FetchTableData(new PageChangeEventArgs { PageNumber = CurrentPage, RowsPerPage = RowPerPage });
        }
    }

    private async Task OnClickRowEdit(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);

        var packageDto = await UnitPackageAppService.GetAsync(id);


        var createUpdatePackage = new CreateUpdateUnitPackageDto
        {
            Package = 
            new CreateUpdatePackageDto
            {


            ArabicName = packageDto.Package.ArabicName,
            Description = packageDto.Package.Description,
            EnglishName = packageDto.Package.EnglishName,
            OrganizationId = packageDto.Package.OrganizationId,
            Price = packageDto.Package.Price,
            Published = packageDto.Package.Published,
            PublishFrom = packageDto.Package.PublishFrom,
            PublishTo = packageDto.Package.PublishTo,
            PackageStatus = packageDto.Package.PackageStatus,
            CustomerCategory = (CustomerCategoryStatus)Enum.Parse(typeof(CustomerCategoryStatus), packageDto.Package.CustomerCategory),
            DurationOfPackage = packageDto.Package.DurationOfPackage,
            Status = packageDto.Package.Status,
            ActivationDate = packageDto.Package.ActivationDate,
            DeactivationDate = packageDto.Package.DectivationDate,
            PackageCategoryType = packageDto.Package.PackageCategoryType,
            TenantId = packageDto.Package.TenantId

            },
            NumberOfSlots = packageDto.NumberOfSlots,
            NumberOfVehicles = packageDto.NumberOfVehicles,

        };



        var parameters = new DialogParameters
        {
            ["UnitPackage"] = createUpdatePackage,
            ["Package"] = createUpdatePackage.Package,
            ["UpdatedId"] = id,
        };


        var dialog = await Dialog.ShowAsync<EditUnitPackageDialog>("Edit Unit Package", parameters);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await FetchTableData(new PageChangeEventArgs { PageNumber = CurrentPage, RowsPerPage = RowPerPage });
        }
    }

    private async Task OnClickRowView(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var dto = await UnitPackageAppService.GetAsync(id);

        var parameters = new DialogParameters { ["Package"] = dto };
        await Dialog.ShowAsync<DetailsUnitPackageDialog>("View Unit Package", parameters);
    }

    private async Task OnClickRowDelete(RowDef row)
    {
        var id = int.Parse(row.Cells[0].Value);
        var name = row.Cells[1].Value;

        var parameters = new DialogParameters { ["Name"] = name };
        var dialog = await Dialog.ShowAsync<SPDialogDelete>("UnitPackageDeleteDialog", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await UnitPackageAppService.DeleteAsync(id);
            Snackbar.Add($"{L["Success"]} {L["Deleted"]} {name}", Severity.Success);
            await FetchTableData(new PageChangeEventArgs { PageNumber = CurrentPage, RowsPerPage = RowPerPage });
        }
    }
}
