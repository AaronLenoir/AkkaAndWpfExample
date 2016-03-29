using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace AkkaAndWpfExample.ThermostatUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        public RelayCommand IncreaseTargetTemperature { get; private set; }

        public RelayCommand DecreaseTargetTemperature { get; private set; }

        private double _temperature = 20;
        public double Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                _temperature = value;
                RaisePropertyChanged<double>(nameof(Temperature));
            }
        }

        private double _targetTemperature = 21;
        public double TargetTemperature
        {
            get
            {
                return _targetTemperature;
            }
            set
            {
                _targetTemperature = value;
                RaisePropertyChanged<double>(nameof(TargetTemperature));
            }
        }
    }
}