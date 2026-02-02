namespace Events.DATA.DTOs.Amwal
{
    /// <summary>
    /// Response model from PayTabs (Amwal) when creating a payment order
    /// </summary>
    public class AmwalCreateOrderResponse
    {
        public string tran_ref { get; set; }
        
        public string tran_type { get; set; }
        
        public string cart_id { get; set; }
        
        public string cart_description { get; set; }
        
        public string cart_currency { get; set; }
        
        public string cart_amount { get; set; }
        
        public string tran_total { get; set; }
        
        public string callback { get; set; }
        
        public string @return { get; set; }
        
        public string redirect_url { get; set; }
        
        public int serviceId { get; set; }
        
        public string paymentChannel { get; set; }
        
        public int profileId { get; set; }
        
        public int merchantId { get; set; }
        
        public string trace { get; set; }
    }
}
