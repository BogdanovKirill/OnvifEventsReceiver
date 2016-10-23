using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;

namespace Common.EventReceiver
{
    public interface IDeviceEventReceiver
    {
        IConnectionParameters ConnectionParameters { get; }

        event EventHandler<DeviceEvent> EventReceived;

        Task ConnectAsync(CancellationToken cancellationToken);

        Task ReceiveAsync(CancellationToken cancellationToken);
    }
}
