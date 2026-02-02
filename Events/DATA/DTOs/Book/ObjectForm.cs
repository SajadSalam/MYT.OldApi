using Events.Entities;

namespace Events.DATA.DTOs.Book;

public class ObjectForm
{
    public List<string> Objects { get; set; }

    public string FullName { get; set; }

    public string PhoneNumber { get; set; }

    public decimal Discount { get; set; } = 0;

    /// <summary>
    /// User's preferred payment method. If not specified, defaults to Amwal.
    /// </summary>
    public PaymentProvider? PreferredPaymentMethod { get; set; }

}