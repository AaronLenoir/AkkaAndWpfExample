namespace AkkaAndWpfExample.ThermostatSystem.Bridge
{
public interface IThermostatView
{
    void UpdateCurrentTemperature(double currentTemperature);
    void UpdateTargetTemperature(double targetTemperature);
}
}
