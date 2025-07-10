using System.Security.Cryptography;
using System.Text;
using ApiServer.Common.Model;
using ApiServer.Utils;
using NetworkProtocols.WebApi;
using NotifyServer.Services.RedisService;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Handlers.SubHandlers;

public class SessionHandler
{
    private const string _TokenEncodeKey = "6<H$}P!ya5)X@=b3tcnE2G<p^^:8Ak)::Q9KQaA%&%p`E)bXp3*9<XLbkEhQWT22";
    private static readonly Encoding _encoding = Encoding.UTF8;
    
    private readonly SessionRedisService _redisService;
    private readonly bool _isExpiry;
    private readonly int _expirySeconds;

    private SessionInfo _sessionInfo;

    public SessionHandler(SessionRedisService redisService, ConfigurationHelper configurationHelper)
    {
        _redisService = redisService;
        
        _isExpiry = configurationHelper.GetValue("SessionExpiry", true);
        _expirySeconds = configurationHelper.GetValue("SessionExpirySeconds", 3600);
    }
    
    private static string _CreateAccessToken(string payload)
    {
        var keyBytes = _encoding.GetBytes(_TokenEncodeKey);
        using var hmac = new HMACSHA256(keyBytes);

        var payloadBytes = _encoding.GetBytes($"{payload} + {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            
        var hashedBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToBase64String(hashedBytes);
    }
    
    public SessionInfo CreateSessionInfo(string loginId, long accountId, byte sequence, byte subSequence, SqlServerDbInfo dbInfo, SqlServerDbInfo slaveDbInfo)
    {
        if(_sessionInfo != null)
            throw new ApiServerException(ResultCode.SystemError, "Session already exists");
        
        var accessToken = _CreateAccessToken(loginId);
        if(string.IsNullOrEmpty(accessToken) == true)
            throw new ApiServerException(ResultCode.SystemError, "Access token is empty");

        _sessionInfo = new SessionInfo
        {
            AccountId = accountId,
            AccessToken = accessToken,
            Sequence = sequence,
            SubSequence = subSequence,
            MainDbInfo = dbInfo,
            SlaveMainDbInfo = slaveDbInfo,
        };

        return _sessionInfo;
    }

    public async Task<SessionInfo> GetSessionInfoAsync(long accountId)
    {
        if(_sessionInfo != null)
            return _sessionInfo;
        
        var sessionKey = SessionInfo.GetSessionKey(accountId);
        var result = await _redisService.StringGetAsync(sessionKey);
        if(result == null || string.IsNullOrEmpty(result) == true)
            throw new ApiServerException(ResultCode.SystemError, "Session not found");
        
        _sessionInfo = SessionInfo.CreateFromJson(result);
        return _sessionInfo;
    }

    public async Task<bool> SetRedisSessionInfo()
    {
        if(_sessionInfo == null)
            throw new ApiServerException(ResultCode.SystemError, "Session info is null");
        
        var expireSeconds = _isExpiry == true ? _expirySeconds : 0;
        var result = await _redisService.StringSetAsync(_sessionInfo.GetKey(), _sessionInfo.ToJson(), ttl: expireSeconds);

        return result;
    }
}