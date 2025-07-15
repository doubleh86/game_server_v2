namespace NetworkProtocols.Shared.Enums;

public enum EventPeriodType
{
    Default = 0,
    WeekDay = 1,
    TimeRange = 2,
    WeekDayAndTimeRange = 3,
}

public enum EventStatus
{
    Expired = 0,    // 종료 ( 클라 전송 안함 )
    Active = 1,     // 진행 중       
    Inactive = 2,   // 이벤트는 진행 중이지만 닫혀 있음
}