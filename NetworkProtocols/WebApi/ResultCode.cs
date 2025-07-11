namespace NetworkProtocols.WebApi;

public enum ResultCode
{
    Ok = 0,
    GameError = 1,
    SystemError = 2,
    DbError = 3
}

public enum DbErrorCode
{
    NotRegisteredFactory = 1,
    ProcedureError = 2,
    TransactionError = 3,
    InParameterWrongType = 4,
}