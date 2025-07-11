using System.Data;
using AdminWeb.Services;
using AdminWeb.Services.Utils;
using BlazorBootstrap;
using BlazorDownloadFile;
using DataTableLoader.Models;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using Microsoft.AspNetCore.Components;
using ServerFramework.CommonUtils.Helper;
using SuperConvert.Extensions;
using DataTableExtensions = AdminWeb.Services.DataTableExtensions;

namespace AdminWeb.Components.Pages;

public partial class ShowTableData : ComponentBase
{
    [Inject] AdminToolServerService _adminToolServerService { get; set; }
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }
    [Inject] private ToastService _toastService { get; set; }
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

    private async Task DownloadCsv(TableDataInfo tableDataInfo)
    {
        var path = $"TableData/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
        
        var serverInfo = _adminToolServerService.GetSqlServerDbInfo(nameof(DataTableDbService));
        var tableType = DataHelper.GetInheritedClassForAdmin(serverInfo.modelAssembly, tableDataInfo.TableName, typeof(BaseData));
        
        var method = typeof(DataHelper).GetMethod("GetDataList", Type.EmptyTypes)?.MakeGenericMethod(tableType);
        if (method == null)
            return;

        var tableListObj = method.Invoke(null, null);
        var extType = typeof(DataTableExtensions);
        var toDataTableMethod = extType.GetMethod("ToDataTable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var genericMethod = toDataTableMethod!.MakeGenericMethod(tableType);
        
        var dataTable = genericMethod.Invoke(null, new[] { tableListObj }) as DataTable;
        if (dataTable == null)
            return;
        
        var csvPath = dataTable.ToCsv(path, fileName: tableDataInfo.TableName);
        if (File.Exists(csvPath) == true)
        {
            var fileBytes = await File.ReadAllBytesAsync(csvPath);
            var downloadResult = await BlazorDownloadFileService.DownloadFile($"{tableDataInfo.TableName}.csv", fileBytes, "application/octet-stream");
            if (downloadResult.Succeeded == true)
            {
                _toastService.Notify(ToastMessageCreator.CreateToastWithTitle(ToastType.Light, "System Information",
                                                                              $"[{tableDataInfo.TableName}] Complete download"));
            }
        }
    }
}