using System.Data;
using DbContext.MainDbContext.DbResultModel.GameDbModels;

namespace DbContext.Common.Models;

public interface IHasCustomTableData<TDataValue>
{
    void SetCustomTableData(DataRow data);
    static abstract DataTable GetDataTable();
    static abstract string GetCustomTableName();
}