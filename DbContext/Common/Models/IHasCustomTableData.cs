using System.Data;
using DbContext.MainDbContext.DbResultModel.GameDbModels;

namespace DbContext.Common.Models;

public interface IHasCustomTableData
{
    static abstract string GetCustomTableName();
    void SetCustomTableData(DataRow row);
    static abstract DataTable GetDataTable();
}