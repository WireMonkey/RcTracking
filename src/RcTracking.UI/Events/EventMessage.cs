using System.ComponentModel.DataAnnotations;

namespace RcTracking.UI.Events
{
    public class EventMessage
    {
        [Required]
        public EventEnum Event { get; init; }
    }
}
