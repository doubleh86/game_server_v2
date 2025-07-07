using System.Text.Json;
using ApiServer.Common.Model;
using ApiServer.Handlers.SubHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands;
using NotifyServer.Services.RedisService;
using RedLockNet;
using ServerFramework.RedisService;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.Controllers;

public abstract class ApiControllerBase: ControllerBase, IDisposable, IAsyncDisposable
{
    protected readonly ApiServerService _service;
    private readonly RedLockManager _lockManager;
    private IRedLock _distributeLock;
    private string _lockResource;
    private SessionHandler _sessionHandler;

    protected ApiControllerBase(ApiServerService service)
    {
        _service = service;
        _lockManager = service.LockManager;
    }

    protected SessionHandler _GetSessionHandler(long accountId)
    {
        if(accountId == 0)
            throw new ArgumentException("accountId cannot be 0");
        
        if(_sessionHandler != null)
            return _sessionHandler;

        var sessionRedis = _service.GetRedisService<SessionRedisService>(accountId);
        _sessionHandler = new SessionHandler(sessionRedis, _service.CustomConfiguration);
        
        return _sessionHandler;
    }

    protected async Task<SqlServerDbInfo> _Initialize(RequestBase request)
    {
        var sessionHandler = _GetSessionHandler(request.AccountId);
        if(sessionHandler == null)
            throw new ApiServerException(ResultCode.SystemError, "Session handler create failed");
        
        var sessionInfo = await _CheckSession(request.Token, request.AccountId, request.Sequence, request.SubSequence);
        if(_InitializeDistributeLock(request.AccountId) == false)
            throw new ApiServerException(ResultCode.SystemError, "Distribute lock create failed");
        
        return sessionInfo.MainDbInfo;
    }

    private async Task <SessionInfo> _CheckSession(string token, long accountId, byte sequence, byte subSequence)
    {
        var sessionInfo = await _sessionHandler.GetSessionInfoAsync(accountId);
        if(sessionInfo == null)
            throw new ApiServerException(ResultCode.SystemError, "Session info is null");
        
        if(sessionInfo.AccessToken != token)
            throw new ApiServerException(ResultCode.SystemError, "Access token does not match");
        
        _CheckSequence(sessionInfo, sequence, subSequence);
        sessionInfo.SetSequence(sequence, subSequence);
        await _sessionHandler.SetRedisSessionInfo();

        return sessionInfo;
    }

    private void _CheckSequence(SessionInfo sessionInfo, byte sequence, byte subSequence)
    {
        var currentSequence = sessionInfo.Sequence + 1;
        if (currentSequence == sequence)
            return;

        if (subSequence < 1)
            throw new ApiServerException(ResultCode.SystemError, "Invalid SubSequence");
        
        throw new ApiServerException(ResultCode.SystemError, "Invalid Sequence");
    }

    private bool _InitializeDistributeLock(long accountId)
    {
        _lockResource = RedLockManager.GetRedLockResource(accountId.ToString());
        _distributeLock = _lockManager.CreateLock(_lockResource);

        if (_distributeLock.IsAcquired == false)
        {
            _service.LoggerService.Warning($"Distribute lock could not be acquired [Status: {_distributeLock.Status}]");
        }
        
        return _distributeLock.IsAcquired;
    }
    
    protected string _OkResponse<TResponse>(ResultCode resultCode, TResponse response) where TResponse : ResponseBase
    {
        response.ResultCode = (int)resultCode;
        return JsonSerializer.Serialize(response);
    }

    protected string _ErrorResponse(ResponseBase response, ResultCode resultCode, string errorMessage)
    {
        response ??= new ResponseBase();
        
        response.DebugMessage = errorMessage;
        return _OkResponse(resultCode, response);
    }

    public void Dispose()
    {
        _distributeLock?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_distributeLock != null) 
            await _distributeLock.DisposeAsync();
    }
}