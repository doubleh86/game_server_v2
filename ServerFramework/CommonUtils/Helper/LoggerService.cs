using Microsoft.Extensions.Configuration;
using Serilog;

namespace ServerFramework.CommonUtils.Helper;

public class LoggerService
{
    private ILogger _logger;
    
    public void CreateLogger(IConfiguration configuration)
    {
        var loggerConfiguration = new LoggerConfiguration();
        loggerConfiguration.ReadFrom.Configuration(configuration);
        
        _logger = loggerConfiguration.CreateLogger();
    }

    public void Error(string message, Exception exception = null)
    {
        if (exception != null)
        {
            _logger?.Error(message, exception);
        }
        else
        {
            _logger?.Error(message);    
        }
    }

    public void Information(string message)
    {
        _logger?.Information(message);
    }

    public void Warning(string message, Exception exception = null)
    {
        if (exception != null)
        {
            _logger?.Warning(message, exception);
        }
        else
        {
            _logger?.Warning(message);
        }
    }

    public void Debug(string message)
    {
        _logger?.Debug(message);
    }
}