using NetworkProtocols.Shared.Enums;

namespace NetworkProtocols.WebApi.ToClientModels;

public class GameEventInfo
{
    public long EventId { get; set; }
    public int EventIndex { get; set; }
    public int EventTypeId { get; set; }
    
    public EventStatus EventStatus { get; set; }
    
    public DateTime StartDateTime { get; set; } // 이벤트 시작 시간
    public DateTime EndDateTime { get; set; }   // 이벤트 종료 시간
    
    public DateTime ExpireDateTime { get; set; }
}

public class GameEventData
{
    public List<GameEventInfo> ProcessList { get; set; }
    public List<GameEventInfo> UpcomingList { get; set; }
    public List<GameEventInfo> UpcomingExpireList { get; set; }
}