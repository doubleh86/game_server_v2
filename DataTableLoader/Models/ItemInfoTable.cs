namespace DataTableLoader.Models;

public class ItemInfoTable : BaseData
{
    public int item_index { get; set; }
    public int item_type { get; set; }
    public int asset_type { get; set; }
    public int item_price { get; set; }
    public int use_amount { get; set; }

    protected override int GetKey()
    {
        return item_index;
    }
    
    public int GetPrice(int count)
    {
        return item_price * count; 
    }
}