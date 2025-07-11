using System.Data;
using DbContext.Common.Models;
using NetworkProtocols.WebApi.ToClientModels;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class InventoryDbResult : IHasCustomTableData, IHasClientModel<InventoryItemInfo>
{
    public int item_index { get; set; }
    public int item_amount { get; set; }
    public DateTime update_date { get; set; }
    public int is_remove { get; set; }
    
    public bool IsRemoved => is_remove == 1;
    
    public static string GetCustomTableName() => "TVP_ItemInfo";
    
    public static InventoryDbResult Create(int itemIndex, int amount)
    {
        return new InventoryDbResult()
        {
            item_index = itemIndex,
            item_amount = amount,
        };
    }

    public void UseItem(int amount)
    {
        item_amount = Math.Max(item_amount - amount, 0);
    }
    public void AddItemAmount(int amount)
    {
        item_amount += amount;
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

    public InventoryItemInfo ToClient()
    {
        return new InventoryItemInfo()
        {
            ItemIndex = item_index,
            ItemAmount = item_amount,
        };
    }
}