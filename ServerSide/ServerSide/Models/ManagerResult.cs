namespace ServerSide.Models;

public class ManagerResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }

    public static ManagerResult<T> Successful(string message, T? data = default)
    {
        return new ManagerResult<T>() { Success = true, Message = message, Data = data };
    }

    public static ManagerResult<T> Unsuccessful(string message, T? data = default)
    {
        return new ManagerResult<T>() { Success = false, Message = message, Data = data };
    }
}
