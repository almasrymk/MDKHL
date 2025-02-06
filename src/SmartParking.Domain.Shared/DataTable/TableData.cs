using System;
using System.Collections.Generic;


namespace SmartParking.DataTable;

public class TableData
{
    public string Name { get; set; }
    public bool ShowHeader { get; set; }
    public bool ShowPagenation { get; set; }
    public int TotalItems { get; set; }
    public List<ColumnDef> Columns { get; set; } = new List<ColumnDef>();
    public List<RowDef> Rows { get; set; } = new List<RowDef>();
}

public class RowDef
{
    public int Index { get; set; }
    public List<KeyValuePair<string, string>> Cells { get; set; } = new List<KeyValuePair<string, string>>();
    public int Height { get; set; }
    public string Foramt { get; set; }


}

public class ColumnDef
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public int Width { get; set; }
    public string PropertyName { get; set; }
    public TypeFilter TypeFilter { get; set; }
    public Type EnumType { get; set; }
    public bool IsColumnVisible { get; set; } = true;
}

