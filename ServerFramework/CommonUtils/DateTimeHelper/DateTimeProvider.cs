namespace ServerFramework.CommonUtils.DateTimeHelper;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; private set; }
    public FakeDateTimeProvider(DateTime fixedTime) => UtcNow = fixedTime;
    public void SetUtcNow(DateTime dateTime) => UtcNow = dateTime;
}