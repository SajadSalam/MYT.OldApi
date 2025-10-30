namespace Events.DATA.DTOs.Category;

public class CategoryDto : BaseDto<Guid>
{
    public string Name { get; set; }

    public string Color { get; set; }
    
    public decimal? Price { get; set; }
}