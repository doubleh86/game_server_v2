namespace ServerFramework.RedisService.Models;

public static class RedisServiceHelper
{
    /// <summary>
    /// 레디스 DB Default 값이 12개인 듯
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static int GetDatabaseId(string typeName)
    {
        return typeName switch
        {
            "SessionRedisService" => 1,
            "WebRedisPublisher" => 2,
            "FrontRedisPublisher" => 3,
            "MatchingRedisPublisher" => 4,
            "PlayRedisPublisher" => 5,
            "GameResultRedisService" => 6,
            "AdminRedisPublisher" => 7,
            _ => 0
        };
    }
}