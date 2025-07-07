using System.Text.Json;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Common.Model;

public class SessionInfo
{
    private const string _Prefix = "UD_";
    
    public string AccessToken { get; set; }
    public string LoginId { get; set; }
    public long AccountId { get; set; }
    public byte Sequence { get; set; }
    public byte SubSequence { get; set; }
        
    public SqlServerDbInfo MainDbInfo { get; set; }
    public string GetKey() => $"{_Prefix}_{AccountId}";
    
    
    public SessionInfo()
    {
        
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static SessionInfo CreateFromJson(string json)
    {
        return JsonSerializer.Deserialize<SessionInfo>(json);
    }

    public static string GetSessionKey(long accountId)
    {
        return $"{_Prefix}_{accountId}";
    }

    public void SetSequence(byte sequence, byte subSequence)
    {
        Sequence = sequence;
        SubSequence = subSequence;
    }
}