namespace RcTracking.UI.Events
{
    public class PlaneFlightAddedMessage : EventMessage
    {
        public Guid PlaneId { get; init; }
    }
}
