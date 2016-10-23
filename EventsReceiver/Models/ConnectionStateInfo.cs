﻿using System;

namespace EventsReceiver.Models
{
    class ConnectionStateInfo
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Description { get; }

        public ConnectionStateInfo(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp.ToLocalTime():HH:mm:ss}]: {Description}";
        }
    }
}
