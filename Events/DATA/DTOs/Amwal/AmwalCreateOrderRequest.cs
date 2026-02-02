namespace Events.DATA.DTOs.Amwal
{
    /// <summary>
    /// Request model for creating a payment order with PayTabs (Amwal)
    /// </summary>
    public class AmwalCreateOrderRequest
    {
        public int profile_id { get; set; }
        
        public string tran_type { get; set; } = "sale";
        
        public string tran_class { get; set; } = "ecom";
        
        public string cart_id { get; set; }
        
        public string cart_description { get; set; }
        
        public string cart_currency { get; set; } = "IQD";
        
        public decimal cart_amount { get; set; }
        
        public string callback { get; set; }
        
        public string @return { get; set; }
    }
}
