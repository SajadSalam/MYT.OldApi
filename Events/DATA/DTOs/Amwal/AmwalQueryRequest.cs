namespace Events.DATA.DTOs.Amwal
{
    /// <summary>
    /// Request model for querying transaction status from PayTabs (Amwal)
    /// </summary>
    public class AmwalQueryRequest
    {
        public int profile_id { get; set; }
        
        public string tran_ref { get; set; }
    }
}
