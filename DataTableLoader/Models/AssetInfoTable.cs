namespace DataTableLoader.Models;

public class AssetInfoTable : BaseData
{
    public int asset_type { get; set; }
    public int default_asset_value { get; set; }

    protected override int GetKey()
    {
        return asset_type;
    }
}
    