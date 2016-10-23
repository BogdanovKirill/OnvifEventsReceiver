using System;
using Common.EventReceiver;
using Devices.Onvif;

namespace Devices
{
    public class DeviceEventReceiverFactory : IDeviceEventReceiverFactory
    {
        public IDeviceEventReceiver Create(IConnectionParameters connectionParameters)
        {
            return new OnvifEventReceiver(connectionParameters, new OnvifClientFactory(), TimeSpan.FromSeconds(60));
        }
    }
}
