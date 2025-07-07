namespace ServerFramework.SqlServerServices.Models;

public enum ServerError
{
    DbError = 1,
    DbProcedureError = 2,
    GameError = 3,
    DbErrorNotRestart = 4,
    DbRequestParameterError = 5,
    UnknownError = 99999
}