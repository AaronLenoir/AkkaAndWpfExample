using Akka.Actor;
using System;

namespace WpfAkkaIntegration.ThermostatSystem.Actors
{
    public class TemperatureSensorActor : ReceiveActor
    {
        public TemperatureSensorActor()
        {
            Become(Idle);
        }

        #region Messages 

        public class StartMeasuring { }

        public class StopMeasuring { }

        public class TakeMeasurement { }

        public class TemperatureMeasured
        {
            public DateTime TimeStamp { get; private set; }
            public double Temperature { get; private set; }
            public string Unit { get; private set; }

            public TemperatureMeasured(double temperature, string unit)
            {
                TimeStamp = DateTime.Now;
                Temperature = temperature;
                Unit = unit;
            }
        }

        #endregion

        #region State

        private ICancelable _schedule;

        #endregion

        #region States

        public void Idle()
        {
            Receive<StartMeasuring>(message => handleStart());
        }

        public void Measuring()
        {
            Receive<TakeMeasurement>(message => HandleTakeMeasurement());
            Receive<StopMeasuring>(message => HandleStop());
        }

        #endregion

        #region Handlers

        private void handleStart()
        {
            _schedule = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(0, 500, Self, new TakeMeasurement(), Self);
            Become(Measuring);
        }

        private void HandleStop()
        {
            _schedule.Cancel();
            Become(Idle);
        }

        private double _temperature = 0.01;
        private void HandleTakeMeasurement()
        {
            // TODO: Implement measurement
            _temperature += 0.01;
            var message = new TemperatureMeasured(Math.Round(_temperature,2), "°C");
            Context.Parent.Tell(message);
        }

        #endregion
    }
}
