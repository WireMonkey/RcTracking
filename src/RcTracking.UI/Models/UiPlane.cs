using System.ComponentModel.DataAnnotations;

namespace RcTracking.UI.Models
{
    public class UiPlane
    {
        [Required]
        public Guid Id { get; init; }

        public int TotalFlights { get; set; } = 0;
        public DateOnly? LastFlight { get; set; }
        public int DaysFlown { get; set; } = 0;
        public int AvgFlightsPerDay { get; set; } = 0;

    }
}
