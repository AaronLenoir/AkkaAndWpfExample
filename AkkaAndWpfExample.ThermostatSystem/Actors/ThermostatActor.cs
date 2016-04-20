using System;
using Akka.Actor;

namespace WpfAkkaIntegration.ThermostatSystem.Actors
{
    public class ThermostatActor : PubSubActor
    {
        public ThermostatActor()
        {
            Become(Active);
        }

        #region Messages

        public class IncreaseTargetTemperature
        {
            public double Step { get; private set; }
            public IncreaseTargetTemperature(double step) { Step = step; }
        }

        public class DecreaseTargetTemperature
        {
            public double Step { get; private set; }
            public DecreaseTargetTemperature(double step) { Step = step; }
        }

        public class TargetTemperatureSet
        {
            public double TargetTemperature { get; private set; }
            public TargetTemperatureSet(double targetTemperature) { TargetTemperature = targetTemperature; }
        }

        #endregion

        #region State

        private double _targetTemperature = 21;
        private IActorRef _temperatureSensor;

        #endregion

        #region States

        public void Active()
        {
            if (_temperatureSensor == null)
            {
                _temperatureSensor = CreateSensor();
                _temperatureSensor.Tell(new TemperatureSensorActor.StartMeasuring());
            }

            Receive<IncreaseTargetTemperature>(message => HandleTargetChange(message.Step));
            Receive<DecreaseTargetTemperature>(message => HandleTargetChange(message.Step * -1));
            Receive<TemperatureSensorActor.TemperatureMeasured>(message => HandleMeasurement(message));

            AcceptSubscribers();
        }

        private IActorRef CreateSensor()
        {
            var props = Props.Create<TemperatureSensorActor>();
            return Context.ActorOf<TemperatureSensorActor>();
        }

        #endregion

        #region Handlers

        private void HandleTargetChange(double step)
        {
            _targetTemperature += step;
            var message = new TargetTemperatureSet(_targetTemperature);
            Publish(message);
        }

        private void HandleMeasurement(TemperatureSensorActor.TemperatureMeasured message)
        {
            Publish(message);
        }

        public override void OnSubscribe(IActorRef subscriber)
        {
            var message = new TargetTemperatureSet(_targetTemperature);
            Publish(message);
        }

        #endregion

    }
}
