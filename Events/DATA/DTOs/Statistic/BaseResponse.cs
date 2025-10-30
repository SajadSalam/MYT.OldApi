namespace Events.DATA.DTOs.Statistic;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public long? TotalCount { get; set; }
}