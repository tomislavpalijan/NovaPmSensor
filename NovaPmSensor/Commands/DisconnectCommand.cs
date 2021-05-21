using NovaPmSensor.ViewModels;

namespace NovaPmSensor.Commands
{
    public class DisconnectCommand : BaseCommand
    {
        private readonly ShellMainViewModel _viewModel;

        public DisconnectCommand(ShellMainViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += (_,_) => { OnCanExecuteChanged(); };
        }

        public override bool CanExecute(object parameter)
        {
            return _viewModel.PmSensor != null && _viewModel.IsConnected;
        }

        public override void Execute(object parameter)
        {
            _viewModel.PmSensor.Close();
        }
    }
}