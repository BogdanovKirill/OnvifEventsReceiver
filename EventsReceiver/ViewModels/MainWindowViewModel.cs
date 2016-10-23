using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Events;
using EventsReceiver.Commands;
using EventsReceiver.Models;
using EventsReceiver.Properties;

namespace EventsReceiver.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IMainWindowModel _mainWindowModel;

        public ObservableCollection<DeviceEvent> Events { get; } = new ObservableCollection<DeviceEvent>();
        public ObservableCollection<string> ConnectionLog { get; } = new ObservableCollection<string>();

        public string DeviceAddress { get; set; } = "http://127.0.0.1";
        public string Login { get; set; } = "root";
        public string Password { get; set; } = "";

        private readonly CommandHandler _startClickCommand;
        public ICommand StartClickCommand => _startClickCommand;

        private readonly CommandHandler _stopClickCommand;
        public ICommand StopClickCommand => _stopClickCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public MainWindowViewModel(IMainWindowModel mainWindowModel)
        {
            if (mainWindowModel == null)
                throw new ArgumentNullException(nameof(mainWindowModel));

            _mainWindowModel = mainWindowModel;

            _startClickCommand = new CommandHandler(OnStartButtonClick, true);
            _stopClickCommand = new CommandHandler(OnStopButtonClick, false);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWindowModelOnEventReceived(object sender, DeviceEvent deviceEvent)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => Events.Add(deviceEvent));
            else
                Events.Add(deviceEvent);
        }

        private void MainWindowModelOnConnectionStateChanged(object sender, ConnectionStateInfo connectionStateInfo)
        {
            string stateInfo = connectionStateInfo.ToString();

            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => ConnectionLog.Add(stateInfo));
            else
                ConnectionLog.Add(stateInfo);
        }

        private void OnStartButtonClick()
        {
            Uri deviceUri;

            string address = DeviceAddress;

            if (!address.StartsWith(HttpGlobals.SchemaPrefix))
                address = HttpGlobals.SchemaPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out deviceUri))
            {
                MessageBox.Show("Bad device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(Login, Password);
            var timeout = TimeSpan.FromSeconds(15);

            var connectionParameters = new ConnectionParameters(deviceUri, credential, timeout);
            _mainWindowModel.ConnectionStateChanged += MainWindowModelOnConnectionStateChanged;
            _mainWindowModel.EventReceived += MainWindowModelOnEventReceived;
            _mainWindowModel.Start(connectionParameters);

            _startClickCommand.SetCanExecute(false);
            _stopClickCommand.SetCanExecute(true);
        }

        private void OnStopButtonClick()
        {
            _mainWindowModel.Stop();
            _mainWindowModel.ConnectionStateChanged -= MainWindowModelOnConnectionStateChanged;
            _mainWindowModel.EventReceived -= MainWindowModelOnEventReceived;

            _stopClickCommand.SetCanExecute(false);
            _startClickCommand.SetCanExecute(true);

            Events.Clear();
            ConnectionLog.Clear();
        }
    }
}
