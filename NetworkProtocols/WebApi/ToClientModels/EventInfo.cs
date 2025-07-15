using NetworkProtocols.Shared.Enums;

namespace NetworkProtocols.WebApi.ToClientModels;

public class EventInfo
{
    public int EventId { get; set; }
    public int EventIndex { get; set; }
    public int EventTypeId { get; set; }
    
    public EventStatus EventStatus { get; set; }
    
    public DateTime StartDateTime { get; set; } // 이벤트 시작 시간
    public DateTime EndDateTime { get; set; }   // 이벤트 종료 시간
    
    public DateTime OpenTime { get; set; }      // 이벤트가 열리는 시간 ( 이벤트 상태가 Inactive의 경우 다음 Open 시간이다. )
    public DateTime CloseTime { get; set; }     // 이벤트가 닫히는 시간 ( 이벤트 상태가 Inactive의 경우 다음 Close 시간이다. )
    
    public DateTime ExpireDateTime { get; set; }
}