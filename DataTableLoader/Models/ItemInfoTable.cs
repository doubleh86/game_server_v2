namespace DataTableLoader.Models;

public class ItemInfoTable : BaseData
{
    public int item_index { get; set; }
    public int item_price { get; set; }
    
    public override int GetKey()
    {
        return item_index;
    }

    public override string GetKeyString()
    {
        return item_index.ToString();
    }
}