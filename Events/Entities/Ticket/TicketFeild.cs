namespace Events.Entities.Ticket;

public class TicketTemplateField
{
    public string? Label { get; set; }
    public string? Key { get; set; }
    public TicketFieldType? Type { get; set; }
    public TextAlignment? Alignment { get; set; }
    public string? FontSize { get; set; }
    public FontWeight? FontWeight { get; set; }
    public string? Color { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? ScaleX { get; set; }
    public string? ScaleY { get; set; }
    public string? Value { get; set; }
    public bool? Required { get; set; }
    public string? ImageURL { get; set; }
    public TextVerticalAlignment? VerticalAlign { get; set; }
    public string? Rotation { get; set; }
    
}

public enum TextVerticalAlignment
{
    Top = 0,
    Middle = 1,
    Bottom = 2
}


public enum TextAlignment
{
    Left = 0,
    Center = 1,
    Right = 2
}

public enum TicketFieldType
{
    Text = 1,
    Image = 2
}


public enum FontWeight
{
    Normal,
    Italic,
    Bold,
    ItalicBold
}



