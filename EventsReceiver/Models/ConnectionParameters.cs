using System;
using System.Net;
using Common.EventReceiver;

namespace EventsReceiver.Models
{
    class ConnectionParameters : IConnectionParameters
    {
        public Uri ConnectionUri { get; }
        public NetworkCredential Credentials { get; }
        public TimeSpan ConnectionTimeout { get; }

        public ConnectionParameters(Uri connectionUri, NetworkCredential credentials, TimeSpan connectionTimeout)
        {
            ConnectionUri = connectionUri;
            Credentials = credentials;
            ConnectionTimeout = connectionTimeout;
        }
    }
}
