using System.Windows;

namespace AkkaAndWpfExample.ThermostatUI
{
public partial class App : Application
{
    private static WpfAkkaIntegration.ThermostatSystem.ThermostatSystem _system = new WpfAkkaIntegration.ThermostatSystem.ThermostatSystem();

    public static WpfAkkaIntegration.ThermostatSystem.ThermostatSystem ThermostatSystem => _system;
}
}
