using Akka.Actor;
using static WpfAkkaIntegration.ThermostatSystem.Actors.ThermostatActor;

namespace AkkaAndWpfExample.ThermostatSystem.Bridge
{
public class ThermostatBridge : IThermostatBridge
{
    private const double fixedTemperatureStep = 1;
    private IActorRef _bridgeActor;

    private readonly IncreaseTargetTemperature increaseMessage = new IncreaseTargetTemperature(fixedTemperatureStep);
    private readonly DecreaseTargetTemperature decreaseMessage = new DecreaseTargetTemperature(fixedTemperatureStep);

    public ThermostatBridge(IActorRef bridgeActor)
    {
        _bridgeActor = bridgeActor;
    }

    public void IncreaseTargetTemperature()
    {
        _bridgeActor.Tell(increaseMessage);
    }

    public void DecreaseTargetTemperature()
    {
        _bridgeActor.Tell(decreaseMessage);

    }
}
}
