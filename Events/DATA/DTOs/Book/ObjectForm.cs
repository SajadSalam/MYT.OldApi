namespace Events.DATA.DTOs.Book;

public class ObjectForm
{
    public List<string> Objects { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public decimal Discount { get; set; } = 0;

}