namespace DbContext.SharedContext.DbResultModel;

public class EventDbResult 
{
    public long event_id { get; set; }
    public int event_type { get; set; }
    public int event_table_index { get; set; }
    public DateTime event_start_date { get; set; }
    public DateTime event_end_date { get; set; }
    public DateTime event_expiry_date { get; set; }
    
}