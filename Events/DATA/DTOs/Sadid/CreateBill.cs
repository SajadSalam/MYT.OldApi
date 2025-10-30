namespace Events.DATA.DTOs.Sadid
{
    public class CreateBillForm
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime? ExpireDate { get; set; }
        public decimal? Amount { get; set; }
    }
}