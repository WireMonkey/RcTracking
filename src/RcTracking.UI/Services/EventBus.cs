using RcTracking.UI.Events;

namespace RcTracking.UI.Services
{
    public class EventBus
    {
        private EventMessage? _lastMessage;
        public EventMessage? Message
        {
            get => _lastMessage;
            set { 
                _lastMessage = value;
                OnMessage?.Invoke(this, value);
            }
        }

        public event EventHandler<EventMessage?>? OnMessage;
    }
}
