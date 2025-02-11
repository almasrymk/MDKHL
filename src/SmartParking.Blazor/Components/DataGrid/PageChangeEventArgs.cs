using System.Collections.Generic;

namespace SmartParking.Blazor.Components.DataGrid;

public class PageChangeEventArgs
{
    public int RowsPerPage { get; set; } = 1;
    public int PageNumber { get; set; } = 1;
    public string SortColumn { get; set; }
    public string SearchTerm { get; set; }
    public List<FilterRequestDto> Filters { get; set; } = new List<FilterRequestDto>();
    public bool IsExport { get; set; }
}
