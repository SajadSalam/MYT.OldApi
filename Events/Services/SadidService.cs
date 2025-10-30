using System.Text;
using Events.DATA.DTOs.Sadid;
using Newtonsoft.Json;
using static Events.Services.SadidService;

namespace Events.Services
{

    public interface ISadidService
    {
        Task<(bool isSuccess, string? id)> CreateBillAsync(CreateBillForm form);
         Task<(bool? success, string? error)> ChangeBillState(string billId, string secretKey, SadidBillState status);
    }

    public class SadidService : ISadidService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public SadidService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _secretKey = configuration["sadid:ticket_key"];
        }

        public async Task<(bool? success, string? error)> ChangeBillState(string billId, string secretKey, SadidBillState status)
        {
            try
            {
                using var httpClient = new HttpClient();

                var payBody = new
                {
                    secretKey,
                    billId,
                    status = (int)status
                };

                var json = JsonConvert.SerializeObject(payBody);
                var content =
                    new StringContent(json, Encoding.UTF8,
                        "application/json-patch+json");


                var response = await httpClient.PutAsync("https://api.sadid.app/api/bills/change-status-with-secret-key",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                return (false, "Some error occurred while changing bill state");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

        public enum SadidBillState
        {
            Paid = 0,
            Unpaid = 1,
            Canceled = 2
        }



        public async Task<(bool isSuccess, string? id)> CreateBillAsync(CreateBillForm billForm)
        {
            var url = "https://api.sadid.app/api/bills/create-bill";

            var body = new
            {
                customerName = billForm.CustomerName,
                customerPhone = billForm.CustomerPhone,
                expireDate = billForm.ExpireDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                redirectUrl = "https://events-api.future-wave.co/api/book/pay",
                amount = billForm.Amount,
            };

            var jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Add("secretKey", _secretKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var billResponse = JsonConvert.DeserializeObject<CreateBillResponse>(responseBody);

                    return (true, billResponse?.BillId);
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                return (false, null);
            }
        }
    }
}