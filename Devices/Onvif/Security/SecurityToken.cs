using System;

namespace Devices.Onvif.Security
{
    class SecurityToken
    {
        public DateTime ServerTime { get; }

        public byte[] NonceBytes { get; }

        public SecurityToken(DateTime serverTime, byte[] nonceBytes)
        {
            ServerTime = serverTime;
            NonceBytes = nonceBytes;
        }
    }
}
