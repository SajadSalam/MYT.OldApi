using Events.DATA.DTOs.Category;

namespace Events.DATA.DTOs.Chart;

public class ChartDto : BaseDto<Guid>
{
    public string Name { get; set; }

    public string Thumbnail { get; set; }

    public string Key { get; set; }

    public List<CategoryDto> Categories { get; set; }
    public bool IsTemplate { get; set; }

}