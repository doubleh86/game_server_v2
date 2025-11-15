using DbContext.SharedContext.DbResultModel;
using MySqlConnector;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.MySqlServices.MySqlCommandModel;
using ServerFramework.MySqlServices.MySqlDapperUtils;

namespace DbContext.SharedContext.MySqlContext.QueryCommands;

public class GetEventInfoListCommandAsync(MySqlDapperServiceBase dbContext, MySqlTransaction transaction = null) 
    : QueryCommandBaseAsync<List<EventDbResult>, EventDbResult>(dbContext, transaction)
{
    
    private const string _GetEventInfoListQuery = "SELECT event_id, event_type_id, event_table_index, event_start_date, " +
                                                  "event_end_date,event_expiry_date, update_date, create_date, " +
                                                  "event_period_type, event_extra_value FROM event_info";
    
    public override async Task<List<EventDbResult>> ExecuteQueryAsync(IDbInParameters inParameters)
    {
        var result = await _RunQueryReturnModelAsync(_GetEventInfoListQuery);
        return result.ToList();
    }
}