using System.Text.Json;
using DataTableLoader.Models;

namespace DataTableLoader.Utils;

public class DataDictionary<TData> where TData : BaseData
{
    private Dictionary<string, TData> _dictionary = new();
    public Dictionary<string, TData> Dictionary => _dictionary;

    public TData GetDataValue(string key)
    {
        return _dictionary.GetValueOrDefault(key, null);
    }

    public bool LoadDataFromDB(DataTableDbService service)
    {
        var dataList = service.LoadData<TData>();
        if (dataList == null)
            return false;

        _dictionary = dataList.ToDictionary(x => x.GetKeyString());
        return true;
    }

    public bool LoadGameDataFromJsonFile(string tableName)
    {
        var filePath = $"./JsonGameData/{typeof(TData).Name}.json";
        using var r = new StreamReader(filePath);
        var jsonString = r.ReadToEnd();
        if (string.IsNullOrEmpty(jsonString) == true)
        {
            return false;
        }
            
        _dictionary = JsonSerializer.Deserialize<Dictionary<string, TData>>(jsonString);
        return true;
    }

    public List<TData> ToValueList()
    {
        return _dictionary.Values.ToList();
    }
}