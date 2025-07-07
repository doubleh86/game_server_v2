using Microsoft.Extensions.Configuration;

namespace ServerFramework.CommonUtils.Helper;

public class ConfigurationHelper
{
    private IConfiguration _configuration;
    public IConfiguration Configuration => _configuration;
    public void Initialize(List<string> configFiles)
    {
        var configurationBuilder = new ConfigurationBuilder();
        foreach (var configFile in configFiles)
        {
            if(File.Exists(configFile) == false)
                continue;
            
            configurationBuilder.AddJsonFile(configFile);    
        }
        
        _configuration = configurationBuilder.Build();
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        return _configuration.GetValue<T>(key) ?? defaultValue;
    }

    public T GetSection<T>(string key) where T: class, new()
    {
        var options = new T();
        _configuration.GetSection(key).Bind(options);

        return options;
    }
}