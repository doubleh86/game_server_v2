

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ServerFramework.RedisService.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class RedisConnectionInfo
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Channel { get; set; }
    public string Password { get; set; }
}

public class RedisSettings
{
    // public Dictionary<string, RedisConnectionInfo> ConnectionInfos { get; set; }
    public Dictionary<string, List<RedisConnectionInfo>> ConnectionInfos { get; set; }
}

public class RedLockConnections
{
    public const string SectionName = "RedLockConnections";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Password { get; set; }
}