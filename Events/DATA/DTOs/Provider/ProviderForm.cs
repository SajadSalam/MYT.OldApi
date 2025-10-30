using System.ComponentModel.DataAnnotations;

namespace Events.DATA.DTOs.Provider
{
    public class ProviderForm
    {

        
        [Required]
        [MinLength(2, ErrorMessage = "FullName must be at least 2 characters")]
        public string? FullName { get; set; }
        
        [Required]
        public string? PhoneNumber { get; set; }
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }
        
   
    }
}