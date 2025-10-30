using System.ComponentModel.DataAnnotations;

namespace Events.DATA.DTOs.roles
{
    public class RoleForm
    {
        [Required]
        public string Name { get; set; }
    }
}