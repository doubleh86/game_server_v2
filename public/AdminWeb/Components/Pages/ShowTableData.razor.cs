using System.Data;
using System.Globalization;
using AdminWeb.Services;
using AdminWeb.Services.Utils;
using BlazorBootstrap;
using BlazorDownloadFile;
using CsvHelper;
using DataTableLoader.Models;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SuperConvert.Extensions;
using DataTableExtensions = AdminWeb.Services.DataTableExtensions;

namespace AdminWeb.Components.Pages;

public partial class ShowTableData : ComponentBase
{
    public class TableDataInfo
    {
        public string TableName { get; set; }
    }

    [Inject] AdminToolServerService _adminToolServerService { get; set; }
    [Inject] private IBlazorDownloadFileService BlazorDownloadFileService { get; set; }
    [Inject] private ToastService _toastService { get; set; }
    

    private List<TableDataInfo> _tableDataList;
    private InputFile _fileInputRef;
    
    private TableDataInfo _selectTableData;
    
    [Inject] private IJSRuntime JS { get; set; }


    private List<TableDataInfo> _GetTableDataList()
    {
        _tableDataList = [];
        foreach (var tableName in DataHelper.GetTableNameList)
        {
            _tableDataList.Add(new TableDataInfo { TableName = tableName });
        }

        return _tableDataList;
    }

    private async Task _DataTableUpload(TableDataInfo tableDataInfo)
    {
        _selectTableData =  tableDataInfo;
        await JS.InvokeVoidAsync("eval", "document.getElementById('fileUploader').click()");
    }

    private async Task _HandleFile(InputFileChangeEventArgs args)
    {
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(args.File.Name);
        if (fileNameWithoutExt != _selectTableData.TableName)
            return;
        
        await using var fileStream = args.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        
        ms.Position = 0;

        using var reader = new StreamReader(ms);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        var serverInfo = _adminToolServerService.GetSqlServerDbInfo(nameof(DataTableDbService));

        var tableName = $"dbo.{_selectTableData.TableName}";
        
        await using var dataDbContext = new DataTableDbService(serverInfo);
        var deleteQuery = $"DELETE FROM {tableName}";
        await dataDbContext.Database.ExecuteSqlRawAsync(deleteQuery);

        int rowsAffected = 0;
        foreach (var row in csv.GetRecords<dynamic>())
        {
            if (row is not IDictionary<string, object> dataDic)
                continue;
            
            var parameters = string.Join(",", dataDic.Values);
            var query = $"INSERT INTO dbo.{_selectTableData.TableName} VALUES ({parameters})";
            var result = await dataDbContext.Database.ExecuteSqlRawAsync(query);
            if(result == 1)
                Interlocked.Increment(ref rowsAffected);
        }
        
        DataHelper.ReloadTableData();

        var message = $"[{_selectTableData.TableName}][RowCount : {rowsAffected}] " + $"Update Complete download";
        _toastService.Notify(ToastMessageCreator.CreateToastWithTitle(ToastType.Light, "System Information", message));
    }

    private async Task _DownloadCsv(TableDataInfo tableDataInfo)
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
        
        var dataTable = genericMethod.Invoke(null, [tableListObj]) as DataTable;
        if (dataTable == null)
            return;
        
        var csvPath = dataTable.ToCsv(path, fileName: tableDataInfo.TableName);
        var result = await FileDownloadHelper.DownloadFile(BlazorDownloadFileService, csvPath, $"{tableDataInfo.TableName}.csv");
        if (result == true)
        {
            _toastService.Notify(ToastMessageCreator.CreateToastWithTitle(ToastType.Light, "System Information",
                                                                          $"[{tableDataInfo.TableName}] Complete download"));
        }
    }
}