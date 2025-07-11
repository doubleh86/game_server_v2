using System.Collections.Concurrent;
using DataTableLoader.Models;
using Microsoft.EntityFrameworkCore;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices;
using ServerFramework.SqlServerServices.Models;

namespace DataTableLoader.Utils;

/// <summary>
/// Table 추가 방법
/// 1. DbSet<T> TableName {get;set;} 추가
/// 2. _RegisterTableDbSet 에 _tableMapping 추가
/// 3. OnModelCreating() 에 modelBuilder 추가
/// </summary>
public class DataTableDbService : SqlServerServiceBase
{
    private readonly LoggerService _loggerService;
    private readonly ConcurrentDictionary<Type, object> _tableMapping = new();
    private DbSet<TestTableData> TestTableData { get; set; }
    private DbSet<EventStoryTable> EventStoryTable { get; set; }
    public DbSet<ItemInfoTable> ItemInfoTable { get; set; }
    private DbSet<AssetInfoTable> AssetInfoTable { get; set; }
    
    public DataTableDbService(SqlServerDbInfo settings, bool isLazyLoading = false, LoggerService logger = null) : base(settings)
    {
        UseLazyLoading(isLazyLoading);
        _loggerService = logger;
        
        _RegisterTableDbSet();
    }

    private void _RegisterTableDbSet()
    {
        _tableMapping.TryAdd(typeof(TestTableData), TestTableData);
        _tableMapping.TryAdd(typeof(EventStoryTable), EventStoryTable);
        _tableMapping.TryAdd(typeof(ItemInfoTable), ItemInfoTable);
        _tableMapping.TryAdd(typeof(AssetInfoTable), AssetInfoTable);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestTableData>(entity => entity.HasKey(data => data.index_no));
        modelBuilder.Entity<EventStoryTable>(entity => entity.HasKey(data => data.index_no));
        modelBuilder.Entity<ItemInfoTable>(entity => entity.HasKey(data => data.item_index));
        modelBuilder.Entity<AssetInfoTable>(entity => entity.HasKey(data => data.asset_type));
    }

    public List<T> LoadData<T>() where T : BaseData
    {
        try
        {
            if (_tableMapping.TryGetValue(typeof(T), out var data) == false)
            {
                _loggerService?.Warning("Need Add _tableMapping Dictionary (Check _RegisterTableDbSet() Method)");
                return null; 
            }
                
            return data is not DbSet<T> dbSet ? null : dbSet.ToList();
        }
        catch (Exception e)
        {
            _loggerService?.Warning($"Exception Load Data [Name {typeof(T).Name}]", e);
            return null;
        }
    }
}