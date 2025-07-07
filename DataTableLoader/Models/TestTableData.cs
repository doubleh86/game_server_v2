namespace DataTableLoader.Models;

public class TestTableData : BaseData
{
    public int index_no { get; set; }
    public int grade { get; set; }
    public int player_level { get; set; }
    
    public override int GetKey()
    {
        return index_no;
    }

    public override string GetKeyString()
    {
        return index_no.ToString();
    }
}