using System.Threading;
using System.Threading.Tasks;
using Common.EventReceiver;
using Common.Events;
using EventsReceiver.Models;
using Moq;
using NUnit.Framework;

namespace EventsReceiver.UnitTests.Models
{
    [TestFixture]
    class MainWindowModelTests
    {
        private const int TestWaitTimeoutMs = 15000;

        [Test]
        public void Start_ConnectionEstablishedAndEventIsReceived_EventReceivedHandlerInvoked()
        {
            var patternEvent = new DeviceEvent("test");
            IDeviceEventReceiver eventReceiverFake = CreateDeviceEventReceiverFake(patternEvent);
            IDeviceEventReceiverFactory eventReceiverFactoryFake = CreateDeviceEventReceiverFactoryFake(eventReceiverFake);

            var autoResetEvent = new AutoResetEvent(false);
            DeviceEvent currentEvent = null;
            var model = new MainWindowModel(eventReceiverFactoryFake);
            model.EventReceived += (sender, @event) =>
            {
                currentEvent = @event;
                autoResetEvent.Set();
            };
            model.Start(CreateConnectionParametersFake());

            Assert.IsTrue(autoResetEvent.WaitOne(TestWaitTimeoutMs));
            Assert.AreSame(patternEvent, currentEvent);
        }

        [Test]
        public void Start_WhenConnectingStateChanges_StateChangedHandlerInvoked()
        {
            IDeviceEventReceiver eventReceiverFake = CreateDeviceEventReceiverFake(null);
            IDeviceEventReceiverFactory eventReceiverFactoryFake = CreateDeviceEventReceiverFactoryFake(eventReceiverFake);

            var autoResetEvent = new AutoResetEvent(false);
            var model = new MainWindowModel(eventReceiverFactoryFake);
            model.ConnectionStateChanged += (sender, @event) => autoResetEvent.Set();
            model.Start(CreateConnectionParametersFake());

            Assert.IsTrue(autoResetEvent.WaitOne(TestWaitTimeoutMs));
        }

        [Test]
        public void Stop_ConnectionEstablishedThenAborted_StoppedEventHandlerInvoked()
        {
            IDeviceEventReceiver eventReceiverFake = CreateDeviceEventReceiverFake(null);
            IDeviceEventReceiverFactory eventReceiverFactoryFake = CreateDeviceEventReceiverFactoryFake(eventReceiverFake);

            var autoResetEvent = new AutoResetEvent(false);
            var model = new MainWindowModel(eventReceiverFactoryFake);
            model.Stopped += (sender, args) => autoResetEvent.Set();
            model.Start(CreateConnectionParametersFake());
            Thread.Sleep(500);
            model.Stop();

            Assert.IsTrue(autoResetEvent.WaitOne(TestWaitTimeoutMs));
        }

        private static IConnectionParameters CreateConnectionParametersFake()
        {
            var connectionParameters = new Mock<IConnectionParameters>();
            return connectionParameters.Object;
        }

        private static IDeviceEventReceiverFactory CreateDeviceEventReceiverFactoryFake(IDeviceEventReceiver eventReceiver)
        {
            var factoryFake = new Mock<IDeviceEventReceiverFactory>();
            factoryFake.Setup(x => x.Create(It.IsNotNull<IConnectionParameters>()))
                .Returns(eventReceiver);

            return factoryFake.Object;
        }

        private static IDeviceEventReceiver CreateDeviceEventReceiverFake(DeviceEvent deviceEvent)
        {
            var deviceEventReceiverFake = new Mock<IDeviceEventReceiver>();
            deviceEventReceiverFake.Setup(x => x.ConnectAsync(It.IsNotNull<CancellationToken>())).
                Returns(Task.CompletedTask);
            deviceEventReceiverFake.Setup(x => x.ReceiveAsync(It.IsNotNull<CancellationToken>())).
                Returns(Task.CompletedTask).
                Raises(x => x.EventReceived += null, deviceEventReceiverFake, deviceEvent);
            deviceEventReceiverFake.Setup(x => x.ConnectionParameters).
                Returns(CreateConnectionParametersFake());

            return deviceEventReceiverFake.Object;
        }
    }
}
