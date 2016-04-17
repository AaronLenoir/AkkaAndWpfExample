using Akka.Actor;

namespace WpfAkkaIntegration.ThermostatSystem
{
    public class ThermostatSystem
    {
        private ActorSystem _system;
        private IActorRef _thermostatActor;

        public ThermostatSystem()
        {
            _system = ActorSystem.Create(nameof(ThermostatSystem));
            _thermostatActor = CreateThermostatActor();
        }

        private IActorRef CreateThermostatActor()
        {
            var props = Props.Create<Actors.ThermostatActor>();
            return _system.ActorOf(props, "thermostat");
        }
    }
}
