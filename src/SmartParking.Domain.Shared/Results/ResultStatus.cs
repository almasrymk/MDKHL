namespace SmartParking.Results;

public enum ResultStatus
{
    Ok = 200,
    Error,
    Forbidden,
    Unauthorized=401,
    Invalid=400,
    NotFound = 404,
    Conflict,
    CriticalError = 500,
    Unavailable
}
