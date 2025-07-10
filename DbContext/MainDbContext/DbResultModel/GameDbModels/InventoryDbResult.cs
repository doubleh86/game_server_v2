namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class InventoryDbResult
{
    public int item_index { get; set; }
    public int item_amount { get; set; }
    public DateTime update_date { get; set; }
    public int is_remove { get; set; }
    
    public bool IsRemoved => is_remove == 1;
}