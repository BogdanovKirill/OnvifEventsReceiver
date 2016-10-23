using System;

namespace Common.Events
{
    public class DeviceEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Message { get; }

        public DeviceEvent(string message)
        {
            Message = message;
        }
    }
}
