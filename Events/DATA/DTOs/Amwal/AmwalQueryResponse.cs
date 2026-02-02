namespace Events.DATA.DTOs.Amwal
{
    /// <summary>
    /// Response model from PayTabs (Amwal) transaction query
    /// </summary>
    public class AmwalQueryResponse
    {
        public string tran_ref { get; set; }
        
        public string cart_id { get; set; }
        
        public string cart_description { get; set; }
        
        public string cart_currency { get; set; }
        
        public string cart_amount { get; set; }
        
        public AmwalCustomerDetails customer_details { get; set; }
        
        public AmwalPaymentResult payment_result { get; set; }
        
        public AmwalPaymentInfo payment_info { get; set; }
    }
}
