using System.ComponentModel.DataAnnotations;

namespace Events.DATA.DTOs.Tag;

public class TagForm
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Image { get; set; }
}