namespace DataTableLoader.Models;

public class PlayerLevelTable : BaseData
{
    public int level { get; set; }
    public int accumulated_exp { get; set; }
    protected override int GetKey()
    {
        return level;
    }
     
}