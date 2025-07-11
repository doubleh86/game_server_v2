using System.Data;
using DbContext.Common.Models;
using NetworkProtocols.WebApi.Commands.User;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class AssetDbResult : IHasCustomTableData, IHasClientModel<AssetInfo>
{
    public int asset_type { get; set; }
    public long amount { get; set; }
    public DateTime update_date { get; set; }
    
    public static string GetCustomTableName() => "TVP_AssetInfo";

    public void UseAsset(int useAmount)
    {
        amount = Math.Max(amount - useAmount, 0);
    }

    public void AddAssetAmount(int addAmount)
    {
        amount += addAmount;
    }

    public static AssetDbResult Create(int assetType, long amount)
    {
        return new AssetDbResult
        {
            asset_type = assetType,
            amount = amount,
            update_date = DateTime.UtcNow
        };
    }
    
    public void SetCustomTableData(DataRow row)
    {
        row[nameof(asset_type)] = asset_type;
        row[nameof(amount)] = amount;
    }

    public static DataTable GetDataTable()
    {
        var result = new DataTable();
        result.Columns.Add(nameof(asset_type), typeof(int));
        result.Columns.Add(nameof(amount), typeof(long));

        return result;
    }

    public AssetInfo ToClient()
    {
        return new AssetInfo()
        {
            AssetType = asset_type,
            Amount = amount,
        };
    }
}