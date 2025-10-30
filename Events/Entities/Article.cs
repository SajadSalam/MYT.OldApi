namespace Events.Entities
{
    public class Article : BaseEntity<int>
    {
        public String? Title { get; set; }
        public String? Description { get; set; }
    }
}