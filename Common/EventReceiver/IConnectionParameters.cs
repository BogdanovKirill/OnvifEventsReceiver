using System;
using System.Net;

namespace Common.EventReceiver
{
    public interface IConnectionParameters
    {
        Uri ConnectionUri { get; }

        NetworkCredential Credentials { get; }

        TimeSpan ConnectionTimeout { get; }
    }
}
