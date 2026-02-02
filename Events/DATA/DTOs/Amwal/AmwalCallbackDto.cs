namespace Events.DATA.DTOs.Amwal
{
    /// <summary>
    /// Callback payload received from PayTabs (Amwal) after payment processing
    /// </summary>
    public class AmwalCallbackDto
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
    
    public class AmwalCustomerDetails
    {
        public string name { get; set; }
        
        public string email { get; set; }
        
        public string phone { get; set; }
        
        public string street1 { get; set; }
        
        public string city { get; set; }
        
        public string state { get; set; }
        
        public string country { get; set; }
        
        public string ip { get; set; }
    }
    
    public class AmwalPaymentResult
    {
        /// <summary>
        /// Response status: "A" = Approved, "H" = Hold, "P" = Pending, "V" = Voided, "E" = Error, "D" = Declined
        /// </summary>
        public string response_status { get; set; }
        
        public string response_code { get; set; }
        
        public string response_message { get; set; }
        
        public string acquirer_message { get; set; }
        
        public string acquirer_rrn { get; set; }
        
        public DateTime? transaction_time { get; set; }
    }
    
    public class AmwalPaymentInfo
    {
        public string card_type { get; set; }
        
        public string card_scheme { get; set; }
        
        public string payment_description { get; set; }
    }
}
