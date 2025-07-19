using System.ComponentModel.DataAnnotations.Schema;

namespace DataTableLoader.Models.EventModels;

public abstract class EventTableBase : BaseData
{
    public enum EventCategory
    {
        GameEvent = 1,
        StoryEvent = 2,
    }
    
    [NotMapped] public abstract EventCategory Category { get; set; }
    public int index_no { get; set; }
    public int event_condition { get; set; }
    public int event_value { get; set; }
    public int reward_table_index { get; set; }
    
    protected override int GetKey() => index_no;
}