using System;
using Common.EventReceiver;
using Common.Events;
using EventsReceiver.Models;
using EventsReceiver.ViewModels;
using Moq;
using NUnit.Framework;

namespace EventsReceiver.UnitTests.ViewModels
{
    [TestFixture]
    class MainWindowViewModelTests
    {
        [Test]
        public void StartClickCommand_CommandExecuted_StartModelMethodCalled()
        {
            var mainWindowModelMock = new Mock<IMainWindowModel>();

            var mainWindowViewModel = new MainWindowViewModel(mainWindowModelMock.Object);
            mainWindowViewModel.StartClickCommand.Execute(null);

            mainWindowModelMock.Verify(x => x.Start(It.IsNotNull<IConnectionParameters>()));
        }

        [Test]
        public void StopClickCommand_CommandExecuted_StopModelMethodIsCalled()
        {
            var mainWindowModelMock = new Mock<IMainWindowModel>();

            var mainWindowViewModel = new MainWindowViewModel(mainWindowModelMock.Object);
            mainWindowViewModel.StopClickCommand.Execute(null);

            mainWindowModelMock.Verify(x => x.Stop());
        }

        [Test]
        public void ConnectionParametersPreparing_DeviceAddressLoginAndPasswordAreSet_ValidParametersAreGivenToStartMethod()
        {
            Uri deviceUri = new Uri("http://127.0.0.1");
            string login = "root";
            string password = "1234";
            var mainWindowModelMock = new Mock<IMainWindowModel>();

            var mainWindowViewModel = new MainWindowViewModel(mainWindowModelMock.Object)
            {
                DeviceAddress = deviceUri.Host,
                Login = login,
                Password = password
            };
            mainWindowViewModel.StartClickCommand.Execute(null);

            mainWindowModelMock.Verify(x => x.Start(It.Is<IConnectionParameters>(p =>
                    p.ConnectionUri == deviceUri && p.Credentials.UserName == login && p.Credentials.Password == password)));
        }

        [Test]
        public void EventHandling_DeviceEventFromModel_EventAddedToEventsCollection()
        {
            var deviceEvent = new DeviceEvent("test");
            var mainWindowModelMock = new Mock<IMainWindowModel>();
            mainWindowModelMock.Setup(x => x.Start(It.IsNotNull<ConnectionParameters>()))
                .Raises(x => x.EventReceived += null, this, deviceEvent);

            var mainWindowViewModel = new MainWindowViewModel(mainWindowModelMock.Object);
            mainWindowViewModel.StartClickCommand.Execute(null);

            Assert.AreEqual(1, mainWindowViewModel.Events.Count);
            Assert.AreSame(deviceEvent, mainWindowViewModel.Events[0]);
        }

        [Test]
        public void StateChangeHandling_WhenStateInfoFromModel_StateInfoAddedToConnectioLog()
        {
            var stateInfo = new ConnectionStateInfo("test");
            var mainWindowModelMock = new Mock<IMainWindowModel>();
            mainWindowModelMock.Setup(x => x.Start(It.IsNotNull<ConnectionParameters>()))
                .Raises(x => x.ConnectionStateChanged += null, this, stateInfo);

            var mainWindowViewModel = new MainWindowViewModel(mainWindowModelMock.Object);
            mainWindowViewModel.StartClickCommand.Execute(null);

            Assert.AreEqual(1, mainWindowViewModel.ConnectionLog.Count);
            Assert.AreEqual(stateInfo.ToString(), mainWindowViewModel.ConnectionLog[0]);
        }
    }
}
