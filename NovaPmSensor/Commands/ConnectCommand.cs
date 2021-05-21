using NovaPmSensor.Exception;
using NovaPmSensor.ViewModels;
using PmSensor.Communication;

namespace NovaPmSensor.Commands
{
    public class ConnectCommand : BaseCommand
    {
        private readonly ShellMainViewModel _viewModel;

        public ConnectCommand(ShellMainViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += (_,_) => { OnCanExecuteChanged(); };
        }

        public override bool CanExecute(object parameter)
        {
            return !_viewModel.IsConnected;
        }

        public override void Execute(object parameter)
        {
            if (string.IsNullOrEmpty(_viewModel.PortName))
            {
                _viewModel.Messages.Add("port name is null or empty!");
                return;
            }

            if (!_viewModel.PortName.ToLower().StartsWith("com"))
            {
                _viewModel.Messages.Add("port name doesn't start with COM !");
                return;
            }

            if (_viewModel.PmSensor == null)
            {
                _viewModel.PmSensor = new ParticleMassSensor(_viewModel.PortName);
                _viewModel.PmSensor.PortOpenChangedEvent += PmSensor_PortOpenChangedEvent;
            }

            _viewModel.PmSensor.Open();
        }

        private void PmSensor_PortOpenChangedEvent(bool obj)
        {
            _viewModel.IsConnected = obj;
            OnCanExecuteChanged();
        }
    }
}