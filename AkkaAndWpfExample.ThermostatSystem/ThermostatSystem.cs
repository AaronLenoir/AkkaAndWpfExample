using Akka.Actor;
using AkkaAndWpfExample.ThermostatSystem.Bridge;
using static WpfAkkaIntegration.ThermostatSystem.Actors.PubSubActor;

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

        public IThermostatBridge CreateThermostatBridge(IThermostatView thermostatView)
        {
            var bridgeActor = CreateBridgeActor(thermostatView);
            _thermostatActor.Tell(new Subscribe(bridgeActor));

            return new ThermostatBridge(bridgeActor);
        }

        private IActorRef CreateBridgeActor(IThermostatView thermostatView)
        {
            var props = Props.Create(() => new BridgeActor(thermostatView, _thermostatActor))
                .WithDispatcher("akka.actor.synchronized-dispatcher");
            return _system.ActorOf(props, "bridge");
        }
    }
}
