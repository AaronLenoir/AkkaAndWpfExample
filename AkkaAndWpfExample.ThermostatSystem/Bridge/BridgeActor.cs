using Akka.Actor;
using static WpfAkkaIntegration.ThermostatSystem.Actors.TemperatureSensorActor;
using static WpfAkkaIntegration.ThermostatSystem.Actors.ThermostatActor;

namespace AkkaAndWpfExample.ThermostatSystem.Bridge
{
    public class BridgeActor : ReceiveActor
    {
        private IThermostatView _thermostatView;
        private IActorRef _thermostatActor;

        public BridgeActor(IThermostatView thermostatView, IActorRef thermostatActor)
        {
            _thermostatView = thermostatView;
            _thermostatActor = thermostatActor;
            Become(Active);
        }

        public void Active()
        {
            Receive<TemperatureMeasured>(message => _thermostatView.UpdateCurrentTemperature(message.Temperature));
            Receive<TargetTemperatureSet>(message => _thermostatView.UpdateTargetTemperature(message.TargetTemperature));
            Receive<IncreaseTargetTemperature>(message => _thermostatActor.Tell(message));
            Receive<DecreaseTargetTemperature>(message => _thermostatActor.Tell(message));
        }
    }
}
