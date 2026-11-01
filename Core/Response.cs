namespace BodegaApp.Core;

public class Response
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static Response Success(string message = "Operación realizada correctamente") => new()
    {
        IsSuccess = true,
        Message = message
    };

    public static Response Failure(string message, params string[] errors) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors.ToList()
    };
}

public class Response<TResult> : Response
{
    public TResult? Result { get; set; }

    public static Response<TResult> Success(TResult result, string message = "Operación realizada correctamente") => new()
    {
        IsSuccess = true,
        Message = message,
        Result = result
    };

    public static new Response<TResult> Failure(string message, params string[] errors) => new()
    {
        IsSuccess = false,
        Message = message,
        Errors = errors.ToList()
    };
}
