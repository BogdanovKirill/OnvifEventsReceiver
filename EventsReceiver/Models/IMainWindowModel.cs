using System;
using Common.EventReceiver;
using Common.Events;

namespace EventsReceiver.Models
{
    internal interface IMainWindowModel
    {
        event EventHandler<ConnectionStateInfo> ConnectionStateChanged;
        event EventHandler<DeviceEvent> EventReceived;
        void Start(IConnectionParameters connectionParameters);
        void Stop();
    }
}