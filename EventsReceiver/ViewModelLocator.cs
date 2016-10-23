using System;
using Devices;
using EventsReceiver.Models;
using EventsReceiver.ViewModels;

namespace EventsReceiver
{
    class ViewModelLocator
    {
        private readonly Lazy<MainWindowViewModel> _mainWindowViewModelLazy = new Lazy<MainWindowViewModel>(CreateMainWindowViewModel);

        public MainWindowViewModel MainWindowViewModel => _mainWindowViewModelLazy.Value;

        private static MainWindowViewModel CreateMainWindowViewModel()
        {
            var receiverFactory = new DeviceEventReceiverFactory();
            var model = new MainWindowModel(receiverFactory);
            return new MainWindowViewModel(model);
        }
    }
}
