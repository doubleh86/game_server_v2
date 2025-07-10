using System.Data;
using DbContext.Common.Models;
using NetworkProtocols.WebApi.ToClientModels;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class InventoryDbResult : IHasCustomTableData<InventoryDbResult>, IHasClientModel<InventoryItemInfo>
{
    public int item_index { get; set; }
    public int item_amount { get; set; }
    public DateTime update_date { get; set; }
    public int is_remove { get; set; }
    
    public bool IsRemoved => is_remove == 1;

    public static InventoryDbResult Create(int itemIndex, int amount)
    {
        return new InventoryDbResult()
        {
            item_index = itemIndex,
            item_amount = amount,
        };
    }

    public void SetTvpData(DataRow row)
    {
        row[nameof(item_index)] = item_index;
        row[nameof(item_amount)] = item_amount;
    }

    public static DataTable ToDatabaseTable()
    {
        var result = new DataTable();
        result.Columns.Add(nameof(item_index), typeof(int));
        result.Columns.Add(nameof(item_amount), typeof(int));

        return result;
    }

    public static DataTable CreateTvpDataTable(List<InventoryDbResult> values)
    {
        var tvpResult = ToDatabaseTable();
        foreach (var value in values)
        {
            var newRow = tvpResult.NewRow();
            value.SetTvpData(newRow);
            tvpResult.Rows.Add(newRow);
        }
        return tvpResult;
    }

    public void SetCustomTableData(DataRow row)
    {
        row[nameof(item_index)] = item_index;
        row[nameof(item_amount)] = item_amount;
    }

    public static DataTable GetDataTable()
    {
        var result = new DataTable();
        result.Columns.Add(nameof(item_index), typeof(int));
        result.Columns.Add(nameof(item_amount), typeof(int));

        return result;
    }

    public static string GetCustomTableName()
    {
        return "TVP_ItemInfo";
    }

    public InventoryItemInfo ToClient()
    {
        return new InventoryItemInfo()
        {
            ItemIndex = item_index,
            ItemAmount = item_amount,
        };
    }
}