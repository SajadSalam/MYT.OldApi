namespace Events.DATA.DTOs;

public class BaseDtoWithoutPagination<T>
{
    public List<T> Data { get; set; }
    
}