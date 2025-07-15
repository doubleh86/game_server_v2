## TimeZoneHelper 사용방법
```csharp
  TimeZoneHelper.Initialize() 호출하여 초기화한다.
  IDateTimeProvider 필요 
    -> DefaultDateTimeProvider
    -> FakeDateTimeProvider 서버 시간 변경 시 필요한 DateTimeProvider
```
- DateTime.UtcNow 사용하지 않고 TimeZoneHelper.UtcNow 사용