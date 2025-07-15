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
    Expired = 0,         // 종료 ( 클라 전송 안함 )
    Active = 1,          // 진행 중
    UpcomingOpen = 2,    // 오픈 예정
    UpcomingExpired = 3, // 만료 예정 ( 이벤트 종료 후 보상 기간 등등...)
}