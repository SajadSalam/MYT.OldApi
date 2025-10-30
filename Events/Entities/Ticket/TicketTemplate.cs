using Newtonsoft.Json;
using System.Collections.Generic;

namespace Events.Entities.Ticket
{
    public class TicketTemplate : BaseEntity<Guid>
    {
        public Guid EventId { get; set; }
        public EventEntity Event { get; set; }
        public string? Image { get; set; }
        public TicketTemplateStage? StageDetails { get; set; }

        private List<TicketTemplateField> _fields = new List<TicketTemplateField>();

        public List<TicketTemplateField>? Fields
        {
            get => _fields;
            set
            {
                if (value != null && value.Count > 0)
                {
                    _fields = value;
                }
                else
                {
                    _fields = new List<TicketTemplateField>();
                }
            }
        }
    }
}