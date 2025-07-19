using System.ComponentModel.DataAnnotations.Schema;

namespace DataTableLoader.Models.EventModels;

public class EventStoryTable : EventTableBase
{
    [NotMapped] public override EventCategory Category { get; set; } = EventCategory.StoryEvent;
    public int event_stage_index { get; set; }
}