using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Common.EventReceiver;
using Common.Events;
using Common.Util;
using Devices.Onvif.Behaviour;
using Devices.Onvif.Contracts.DeviceManagement;
using Devices.Onvif.Contracts.Events;
using Devices.Onvif.Security;
using Capabilities = Devices.Onvif.Contracts.DeviceManagement.Capabilities;
using DateTime = System.DateTime;

namespace Devices.Onvif
{
    class OnvifEventReceiver : IDeviceEventReceiver
    {
        private const string DefaultDeviceServicePath = "/onvif/device_service";
        private readonly TimeSpan _subscriptionTerminationTime;

        private readonly IConnectionParameters _connectionParameters;
        private readonly IOnvifClientFactory _onvifClientFactory;
        private readonly string _deviceServicePath;
        private Capabilities _deviceCapabilities;

        public IConnectionParameters ConnectionParameters => _connectionParameters;

        public event EventHandler<DeviceEvent> EventReceived;

        public OnvifEventReceiver(IConnectionParameters connectionParameters, IOnvifClientFactory onvifClientFactory, TimeSpan subscriptionTerminationTime)
        {
            if (connectionParameters == null)
                throw new ArgumentNullException(nameof(connectionParameters));
            if (onvifClientFactory == null)
                throw new ArgumentNullException(nameof(onvifClientFactory));

            _connectionParameters = connectionParameters;
            _onvifClientFactory = onvifClientFactory;
            _subscriptionTerminationTime = subscriptionTerminationTime;

            _deviceServicePath = connectionParameters.ConnectionUri.AbsolutePath;

            if (_deviceServicePath == "/")
                _deviceServicePath = DefaultDeviceServicePath;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            DateTime deviceTime = await GetDeviceTimeAsync();

            if (!_connectionParameters.Credentials.IsEmpty())
            {
                byte[] nonceBytes = new byte[20];
                var random = new Random();
                random.NextBytes(nonceBytes);

                var token = new SecurityToken(deviceTime, nonceBytes);

                _onvifClientFactory.SetSecurityToken(token);
            }

            cancellationToken.ThrowIfCancellationRequested();

            _deviceCapabilities = await GetDeviceCapabilitiesAsync();

            if (!_deviceCapabilities.Events.WSPullPointSupport)
                throw new DeviceEventReceiverException("Device doesn't support pull point subscription");
        }

        public async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            var eventServiceUri = new Uri(_deviceCapabilities.Events.XAddr);
            EndpointAddress endPointAddress = await GetSubscriptionEndPointAddress(eventServiceUri);

            await PullPointAsync(endPointAddress, cancellationToken);
        }
        
        protected void OnEventReceived(DeviceEvent e)
        {
            EventReceived?.Invoke(this, e);
        }

        private async Task PullPointAsync(EndpointAddress endPointAddress, CancellationToken cancellationToken)
        {
            var pullPointSubscriptionClient = _onvifClientFactory.CreateClient<PullPointSubscription>(endPointAddress, _connectionParameters,
                    MessageVersion.Soap12WSAddressing10);
            var subscriptionManagerClient = _onvifClientFactory.CreateClient<SubscriptionManager>(endPointAddress, _connectionParameters,
                MessageVersion.Soap12WSAddressing10);

            var pullRequest = new PullMessagesRequest("PT1S", 1024, null);

            int renewIntervalMs = (int)(_subscriptionTerminationTime.TotalMilliseconds / 2);
            int lastTimeRenewMade = Environment.TickCount;

            while (!cancellationToken.IsCancellationRequested)
            {
                PullMessagesResponse response = await pullPointSubscriptionClient.PullMessagesAsync(pullRequest);

                foreach (var messageHolder in response.NotificationMessage)
                {
                    if (messageHolder.Message == null)
                        continue;

                    var @event = new DeviceEvent(messageHolder.Message.InnerXml);
                    OnEventReceived(@event);
                }
                
                if (TimeUtil.IsTimeOver(lastTimeRenewMade, renewIntervalMs))
                {
                    lastTimeRenewMade = Environment.TickCount;
                    var renew = new Renew {TerminationTime = GetTerminationTime()};
                    await subscriptionManagerClient.RenewAsync(new RenewRequest(renew));
                }
            }
            
            await subscriptionManagerClient.UnsubscribeAsync(new UnsubscribeRequest(new Unsubscribe()));
        }

        private async Task<EndpointAddress> GetSubscriptionEndPointAddress(Uri eventServiceUri)
        {
            var portTypeClient = _onvifClientFactory.CreateClient<EventPortType>(eventServiceUri, 
                _connectionParameters, MessageVersion.Soap12WSAddressing10);

            string terminationTime = GetTerminationTime();
            var subscriptionRequest = new CreatePullPointSubscriptionRequest(null, terminationTime, null, null);
            CreatePullPointSubscriptionResponse response =
                await portTypeClient.CreatePullPointSubscriptionAsync(subscriptionRequest);

            var subscriptionRefUri = new Uri(response.SubscriptionReference.Address.Value);

            var adressHeaders = new List<AddressHeader>();

            if (response.SubscriptionReference.ReferenceParameters?.Any != null)
                foreach (System.Xml.XmlElement element in response.SubscriptionReference.ReferenceParameters.Any)
                    adressHeaders.Add(new CustomAddressHeader(element));

            var seviceUri = GetServiceUri(subscriptionRefUri.PathAndQuery);
            var endPointAddress = new EndpointAddress(seviceUri, adressHeaders.ToArray());
            return endPointAddress;
        }

        private async Task<DateTime> GetDeviceTimeAsync()
        {
            Device deviceClient = CreateDeviceClient();
            SystemDateTime deviceSystemDateTime = await deviceClient.GetSystemDateAndTimeAsync();

            DateTime deviceTime;
            if (deviceSystemDateTime.UTCDateTime == null)
                deviceTime = DateTime.UtcNow;
            else
            {
                deviceTime = new DateTime(deviceSystemDateTime.UTCDateTime.Date.Year,
                    deviceSystemDateTime.UTCDateTime.Date.Month,
                    deviceSystemDateTime.UTCDateTime.Date.Day, deviceSystemDateTime.UTCDateTime.Time.Hour,
                    deviceSystemDateTime.UTCDateTime.Time.Minute, deviceSystemDateTime.UTCDateTime.Time.Second, 0,
                    DateTimeKind.Utc);
            }

            return deviceTime;
        }

        private async Task<Capabilities> GetDeviceCapabilitiesAsync()
        {
            Device deviceClient = CreateDeviceClient();

            GetCapabilitiesResponse capabilitiesResponse =
                await deviceClient.GetCapabilitiesAsync(new GetCapabilitiesRequest(new[] { CapabilityCategory.All }));

            return capabilitiesResponse.Capabilities;
        }

        private Device CreateDeviceClient()
        {
            Uri deviceServiceUri = GetServiceUri(_deviceServicePath);

            var deviceClient = _onvifClientFactory.CreateClient<Device>(deviceServiceUri, 
                _connectionParameters, MessageVersion.Soap12);

            return deviceClient;
        }

        private Uri GetServiceUri(string serviceRelativePath)
        {
            return new Uri(_connectionParameters.ConnectionUri, serviceRelativePath);
        }

        private string GetTerminationTime()
        {
            return $"PT{(int)_subscriptionTerminationTime.TotalSeconds}S";
        }
    }
}
