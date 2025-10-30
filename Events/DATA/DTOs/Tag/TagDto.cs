namespace Events.DATA.DTOs.Tag;

public class TagDto : BaseDto<Guid>
{
    public string? Name { get; set; }

    public string? Image { get; set; }

    public int? NumberOfEvent { get; set; }
    
}