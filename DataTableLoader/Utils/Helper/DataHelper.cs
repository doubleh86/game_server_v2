using System.Collections.Concurrent;
using System.Reflection;
using DataTableLoader.Models;
using JetBrains.Annotations;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace DataTableLoader.Utils.Helper;

public static partial class DataHelper
{
    private static SqlServerDbInfo _settings;
    private static LoggerService _loggerService;
    private static readonly ConcurrentDictionary<string, object> _dataDictionary = new();
    
    public static ICollection<string> GetTableNameList => _dataDictionary.Keys;

    public static void Initialize(SqlServerDbInfo settings, LoggerService logger)
    {
        _settings = settings;
        _loggerService = logger;
    }

    public static void LoadAllTableData()
    {
        _dataDictionary.Clear();
        using var dbService = new DataTableDbService(_settings, logger:_loggerService);

        var classTypes = _GetInheritedClasses(dbService.ModelAssembly, typeof(BaseData));
        if (classTypes == null || classTypes.Count < 1)
            return;
        
        var method = typeof(DataHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                                                           .Where(x => x.Name == nameof(_InitializeData))
                                                           .FirstOrDefault(x => x.GetParameters().Length == 1);
        if (method == null)
            return;
        
        foreach (var classType in classTypes)
        {
            var generic = method.MakeGenericMethod(classType);
            generic.Invoke(null, [dbService]);
        }
        
        _loggerService?.Information("Loaded all table data");
    }

    private static bool _InitializeData<T>() where T : BaseData
    {
        if (_dataDictionary.ContainsKey(typeof(T).Name) == true)
            return true;

        using var dbService = new DataTableDbService(_settings, logger:_loggerService);
        return _InitializeData<T>(dbService);
    }

    private static bool _InitializeData<T>(DataTableDbService service) where T : BaseData
    {
        if (_dataDictionary.ContainsKey(typeof(T).Name) == true)
            return true;
        
        var dictionary = new DataDictionary<T>();
        if (dictionary.LoadDataFromDB(service) == false)
        {
            _loggerService.Warning($"DataHelper._InitializeData Failed [{typeof(T).Name}]");
            return false;
        }
        
        _dataDictionary.TryAdd(typeof(T).Name, dictionary);
        return true;
    }

    private static List<Type> _GetInheritedClasses(string modelAssembly, Type baseType)
    {
        var splitInfo = modelAssembly.Split(',');
        
        var namespaceName = splitInfo[0].Trim();
        var assemblyName = splitInfo[1].Trim();
        
        var assembly = Assembly.Load(assemblyName);
        var types = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false } 
                                                             && t.Namespace == namespaceName 
                                                             && baseType.IsAssignableFrom(t));
        
        return types.ToList();
    }

    private static DataDictionary<T> _GetDataDictionary<T>() where T : BaseData
    {
        if (_InitializeData<T>() == false)
        {
            return null;
        }
        
        return _dataDictionary[typeof(T).Name] as DataDictionary<T>;
    }

    public static List<T> GetDataList<T>() where T : BaseData
    {
        var dictionary = _GetDataDictionary<T>();
        return dictionary?.ToValueList();
    }

    public static T GetData<T>(string key) where T : BaseData
    {
        var dictionary = _GetDataDictionary<T>();
        return dictionary?.GetDataValue(key);
    }
}