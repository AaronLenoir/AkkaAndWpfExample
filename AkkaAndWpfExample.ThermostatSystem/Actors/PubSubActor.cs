using System;
using System.Threading.Tasks;
using Akka.Actor;
using System.Collections.Generic;

namespace WpfAkkaIntegration.ThermostatSystem.Actors
{
    public class PubSubActor : ReceiveActor
    {

        #region Messages

        public class Subscribe
        {
            public IActorRef Subscriber { get; private set; }
            public Subscribe(IActorRef subscriber)
            {
                Subscriber = subscriber;
            }
        }

        #endregion

        #region States

        public void AcceptSubscribers()
        {
            Receive<Subscribe>(message => handleSubscribe(message));
        }

        #endregion

        #region State

        private List<IActorRef> _subscribers = new List<IActorRef>();

        #endregion

        #region Handlers

        private void handleSubscribe(Subscribe message)
        {
            _subscribers.Add(message.Subscriber);
        }

        #endregion

        public void Publish(object message)
        {
            foreach(var subscriber in _subscribers)
            {
                subscriber.Tell(message);
            }
        }
    }
}
