namespace Common.EventReceiver
{
    public interface IDeviceEventReceiverFactory
    {
        IDeviceEventReceiver Create(IConnectionParameters connectionParameters);
    }
}
