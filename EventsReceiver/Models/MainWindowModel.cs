using System;
using System.Threading;
using System.Threading.Tasks;
using Common.EventReceiver;
using Common.Events;

namespace EventsReceiver.Models
{
    class MainWindowModel : IMainWindowModel
    {
        private readonly IDeviceEventReceiverFactory _deviceEventReceiverFactory;
        private static readonly TimeSpan ReconnectionDelay = TimeSpan.FromSeconds(5);
        private CancellationTokenSource _cancellationTokenSource;
        private IDeviceEventReceiver _deviceEventReceiver;

        public event EventHandler<ConnectionStateInfo> ConnectionStateChanged; 
        public event EventHandler<DeviceEvent> EventReceived;
        public event EventHandler Stopped; 

        public MainWindowModel(IDeviceEventReceiverFactory deviceEventReceiverFactory)
        {
            if (deviceEventReceiverFactory == null)
                throw new ArgumentNullException(nameof(deviceEventReceiverFactory));

            _deviceEventReceiverFactory = deviceEventReceiverFactory;
        }

        public void Start(IConnectionParameters connectionParameters)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            _deviceEventReceiver = _deviceEventReceiverFactory.Create(connectionParameters);
            _deviceEventReceiver.EventReceived += DeviceEventReceiverOnEventReceived;

            Task.Run(() => ReceiveEventsAsync(_deviceEventReceiver, _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (_deviceEventReceiver == null)
                return;

            _cancellationTokenSource.Cancel();
            _deviceEventReceiver.EventReceived -= DeviceEventReceiverOnEventReceived;
            _deviceEventReceiver = null;
        }

        protected virtual void OnStateChanged(ConnectionStateInfo e)
        {
            ConnectionStateChanged?.Invoke(this, e);
        }

        private async void ReceiveEventsAsync(IDeviceEventReceiver deviceEventReceiver, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    OnStateChanged(new ConnectionStateInfo(
                            $"Connecting to {deviceEventReceiver.ConnectionParameters.ConnectionUri}..."));

                    await deviceEventReceiver.ConnectAsync(token).ConfigureAwait(false);

                    OnStateChanged(new ConnectionStateInfo("Connection is established. Receiving..."));

                    await deviceEventReceiver.ReceiveAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    OnStateChanged(new ConnectionStateInfo($"Connection error: {e.Message}"));
                }

                try
                {
                    await Task.Delay(ReconnectionDelay, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Stopped?.Invoke(this, EventArgs.Empty);
        }
        
        private void DeviceEventReceiverOnEventReceived(object sender, DeviceEvent deviceEvent)
        {
            EventReceived?.Invoke(sender, deviceEvent);
        }
    }
}
