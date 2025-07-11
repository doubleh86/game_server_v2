namespace DataTableLoader.Models;

public class EventStoryTable : BaseData
{
    public int index_no { get; set; }
    public int event_stage_index { get; set; }
    public int reward_table_index { get; set; }

    protected override int GetKey()
    {
        return index_no;
    }
}