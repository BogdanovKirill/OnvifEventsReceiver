using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Common.EventReceiver;
using Devices.Onvif;
using Devices.Onvif.Contracts.DeviceManagement;
using Devices.Onvif.Contracts.Events;
using Devices.Onvif.Security;
using Moq;
using NUnit.Framework;
using Capabilities = Devices.Onvif.Contracts.DeviceManagement.Capabilities;

namespace Devices.UnitTests.Onvif
{
    [TestFixture]
    class OnvifEventReceiverTests
    {
        [Test]
        public async void ConnectAsync_FirstStage_GetSystemDateAndTimeIsCalled()
        {
            var connectionParameters = CreateConnectionParametersFake();
            var deviceMock = CreateDeviceMock();
            var onvifClientFactoryFake = CreateOnvifClientFactoryMock(deviceMock.Object, null, null);

            var receiver = new OnvifEventReceiver(connectionParameters, onvifClientFactoryFake.Object, TimeSpan.Zero);
            await receiver.ConnectAsync(CancellationToken.None);

            deviceMock.Verify(x => x.GetSystemDateAndTimeAsync());
        }

        [Test]
        public async void ConnectAsync_SecondStage_GetCapabilitiesIsCalled()
        {
            var connectionParameters = CreateConnectionParametersFake();
            var deviceMock = CreateDeviceMock();
            var onvifClientFactoryFake = CreateOnvifClientFactoryMock(deviceMock.Object, null, null);

            var receiver = new OnvifEventReceiver(connectionParameters, onvifClientFactoryFake.Object, TimeSpan.Zero);
            await receiver.ConnectAsync(CancellationToken.None);

            deviceMock.Verify(x => x.GetCapabilitiesAsync(It.IsAny<GetCapabilitiesRequest>()));
        }

        [Test]
        public async void ConnectAsync_LoginAndPasswordAreSet_SecurityTokenCreated()
        {
            var connectionParameters = CreateConnectionParametersFake();
            var deviceFake = CreateDeviceMock();
            var onvifClientFactoryMock = CreateOnvifClientFactoryMock(deviceFake.Object, null, null);

            var receiver = new OnvifEventReceiver(connectionParameters, onvifClientFactoryMock.Object, TimeSpan.Zero);
            await receiver.ConnectAsync(CancellationToken.None);

            onvifClientFactoryMock.Verify(x => x.SetSecurityToken(It.IsNotNull<SecurityToken>()));
        }

        [Test]
        public async void ReceiveAsync_PullPointSubscription_PullMessagesThenRenewAreCalled()
        {
            var connectionParameters = CreateConnectionParametersFake();
            var deviceFake = CreateDeviceMock();

            var cancellationTokeSource = new CancellationTokenSource();

            var pullPointSubscriptionMock = new Mock<PullPointSubscription>();
            pullPointSubscriptionMock.Setup(x => x.PullMessagesAsync(It.IsNotNull<PullMessagesRequest>())).
                Returns(Task.FromResult(new PullMessagesResponse(new System.DateTime(), new System.DateTime(), new NotificationMessageHolderType[0]))).
                Callback(() => cancellationTokeSource.Cancel());

            var subscriptionManagerMock = new Mock<SubscriptionManager>();

            var onvifClientFactoryFake = CreateOnvifClientFactoryMock(deviceFake.Object, pullPointSubscriptionMock.Object, subscriptionManagerMock.Object);

            var receiver = new OnvifEventReceiver(connectionParameters, onvifClientFactoryFake.Object, TimeSpan.FromSeconds(-1));
            await receiver.ConnectAsync(cancellationTokeSource.Token);
            await receiver.ReceiveAsync(cancellationTokeSource.Token);

            pullPointSubscriptionMock.Verify(x => x.PullMessagesAsync(It.IsNotNull<PullMessagesRequest>()));
            subscriptionManagerMock.Verify(x => x.RenewAsync(It.IsNotNull<RenewRequest>()));
        }

        private Mock<IOnvifClientFactory> CreateOnvifClientFactoryMock(Device device, PullPointSubscription pullPointSubscription, SubscriptionManager subscriptionManager)
        {
            var onvifClientFactoryMock = new Mock<IOnvifClientFactory>();

            onvifClientFactoryMock.Setup(x => x.CreateClient<Device>(It.IsNotNull<Uri>(),
                It.IsNotNull<IConnectionParameters>(), It.IsAny<MessageVersion>())).
                Returns(device);
            onvifClientFactoryMock.Setup(x => x.CreateClient<EventPortType>(It.IsNotNull<Uri>(),
                It.IsNotNull<IConnectionParameters>(), It.IsAny<MessageVersion>())).
                Returns(CreatEeventPortTypeFake);
            onvifClientFactoryMock.Setup(x => x.CreateClient<PullPointSubscription>(It.IsNotNull<EndpointAddress>(),
                It.IsNotNull<IConnectionParameters>(), It.IsAny<MessageVersion>())).
                Returns(pullPointSubscription);
            onvifClientFactoryMock.Setup(x => x.CreateClient<SubscriptionManager>(It.IsNotNull<EndpointAddress>(),
                It.IsNotNull<IConnectionParameters>(), It.IsAny<MessageVersion>())).
                Returns(subscriptionManager);

            return onvifClientFactoryMock;
        }

        private EventPortType CreatEeventPortTypeFake()
        {
            var endPointRefType = new EndpointReferenceType
            {
                Address = new AttributedURIType {Value = "http://127.0.0.1"}
            };

            var response = new CreatePullPointSubscriptionResponse(endPointRefType, new System.DateTime(), null, null);

            var eventPortTypeMock = new Mock<EventPortType>();
            eventPortTypeMock.Setup(
                x => x.CreatePullPointSubscriptionAsync(It.IsNotNull<CreatePullPointSubscriptionRequest>())).
                Returns(Task.FromResult(response));

            return eventPortTypeMock.Object;
        }

        private Mock<Device> CreateDeviceMock()
        {
            var capabilities = new Capabilities { Events = new EventCapabilities { XAddr = "http://127.0.0.1", WSPullPointSupport = true}};

            var deviceMock = new Mock<Device>();
            deviceMock.Setup(x => x.GetCapabilitiesAsync(It.IsNotNull<GetCapabilitiesRequest>())).
                Returns(Task.FromResult(new GetCapabilitiesResponse(capabilities)));
            deviceMock.Setup(x => x.GetSystemDateAndTimeAsync()).
                Returns(Task.FromResult(new SystemDateTime()));

            return deviceMock;
        }

        private IConnectionParameters CreateConnectionParametersFake()
        {
            var connectionParameters = new Mock<IConnectionParameters>();
            connectionParameters.Setup(x => x.ConnectionUri).Returns(new Uri("http://127.0.0.1"));
            connectionParameters.Setup(x => x.Credentials).Returns(new NetworkCredential("root", "pass"));

            return connectionParameters.Object;
        }
    }
}
