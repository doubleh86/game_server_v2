using System.ComponentModel.DataAnnotations.Schema;

namespace DataTableLoader.Models.EventModels;

public class GameEventTable : EventTableBase
{
    [NotMapped] public override EventCategory Category { get; set; } = EventCategory.GameEvent;
}