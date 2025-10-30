using System.ComponentModel.DataAnnotations;

namespace Events.Entities
{
    public class BaseEntity<TId>
    {
        [Key]
        public TId Id { get; set; }

        public bool Deleted { get; set; } = false;
        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
    }
}