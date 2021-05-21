using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Input;
using NovaPmSensor.Commands;
using PmSensor.Communication;

namespace NovaPmSensor.ViewModels
{
    public class ShellMainViewModel : ViewModelBase
    { 
        public ParticleMassSensor PmSensor { get; internal set; }

        private bool _isConnected;
        private string _portName;

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected == value)
                    return;

                _isConnected = value;

                OnPropertyChanged(nameof(IsConnected));
                Messages.Add($"connection changed open : {_isConnected}");
            }
        }

        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
                OnPropertyChanged(nameof(PortName));
            }
        }


        public ObservableCollection<string> SerialPortNames { get; set; }
        public ObservableCollection<string> Messages { get; set; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        public ShellMainViewModel()
        {
            ConnectCommand = new ConnectCommand(this);
            DisconnectCommand = new DisconnectCommand(this);
            SerialPortNames = new ObservableCollection<string>(SerialPort.GetPortNames());
            Messages = new ObservableCollection<string>();
        }
    }
}