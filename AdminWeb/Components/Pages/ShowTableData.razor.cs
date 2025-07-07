using DataTableLoader.Utils.Helper;
using Microsoft.AspNetCore.Components;

namespace AdminWeb.Components.Pages;

public partial class ShowTableData : ComponentBase
{
    public class TableDataInfo
    {
        public string TableName { get; set; }
    }
    
    private List<TableDataInfo> _tableDataList;

    private List<TableDataInfo> _GetTableDataList()
    {
        _tableDataList = [];
        foreach (var tableName in DataHelper.GetTableNameList)
        {
            _tableDataList.Add(new TableDataInfo { TableName = tableName });
        }
        
        return _tableDataList;
    }
}