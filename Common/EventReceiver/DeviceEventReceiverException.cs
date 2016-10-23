using System;
using System.Runtime.Serialization;

namespace Common.EventReceiver
{
    [Serializable]
    public class DeviceEventReceiverException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DeviceEventReceiverException()
        {
        }

        public DeviceEventReceiverException(string message) : base(message)
        {
        }

        public DeviceEventReceiverException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DeviceEventReceiverException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
