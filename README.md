# AkkaAndWpfExample

Code backing up a blogpost on integrating Akka.NET and WPF. This branch is the starting point, the solution I came up with is in the "bridged" branch.

The post: https://blog.aaronlenoir.com/2016/04/20/integrating-wpf-and-akka-net-2

Full text:

I'm wondering how to combine an Akka.NET actor system with a WPF front-end.

The [Actor Model](http://doc.akka.io/docs/akka/snapshot/general/actors.html) provides a nice way to build concurrent systems. There are several implementations for .NET. [Orleans](https://github.com/dotnet/orleans) and [Akka.Net](http://getakka.net/) are the best known.

As an excercise, let's build a "thermostat" system.

I'll leave out some implementation details so we can focus on the integration.

## The Thermostat System

The system will display both the current and the desired temperature to the user.

It'll have two buttons to increase and decrease the desired temperature.

In the background, the system should control the heating but we'll ignore that bit here.

### UI

We'll create a basic WPF application with MVVM. The ViewModel will have a property for the current temperature and the target temperature.

It also accepts two commands: ```IncreaseTargetTemperature``` and ```DecreaseTargetTemperature```.

### Back-end

The back-end should have a thermometer that takes measurements on a regular basis.

It should also keep track of the target temperature, set by the user.

The target temperature can change by two messages sent to the system:

* Increase the target temperature
* Reduce the target temperature

As output, the system should emit:

* The current temperature, when a measurement takes place
* The target temperature, when it's changed

## Thermostat Actor System

### Actors

This simple actor system can consist of two Actors:

* TemperatureSensorActor
* ThermostatActor

### TemperatureSensor

The TemperatureSensor sends a message ```TemperatureMeasured``` to its Parent actor. This happens as a response to a ```TakeMeasurement``` message.

The TemperatureSensor sends the ```TakeMeasurement``` to itself on a fixed interval.

### Thermostat

The Thermostat is in charge of creating a TemperatureSensor.

It's a ["pub-sub"](https://github.com/petabridge/akka-bootcamp/blob/0ac15bdc4dbe54f9169e8e6e026bf0ec28e8f2a2/src/Unit-2/lesson3/README.md#how-do-i-do-pubsub-with-akkanet-actors) actor that should send to its subscribers the following messages:

* ```TargetTemperatureSet```
* ```TemperatureMeasured```

## Bridge

With the UI and the actor system ready, we can start to make them talk with each other.

> The code we start with is on [github](https://github.com/AaronLenoir/AkkaAndWpfExample/tree/master). If you want to try this yourself first, you could fork it. If not, just read on to see how I do it.

To support this, WPF should react to messages sent by the Actors. At the same time, WPF should be able to send messages to the Actor System, in some way.

To support this communication we could create a "Bridge". The Bridge will contain:

* A Bridge Actor
* One or more public functions

The Bridge Actor will have a reference to the ViewModel. The ViewModel will have a reference to the Bridge (not the Bridge Actor!).

The Bridge can expose methods the ViewModel can call which pass messages to the Actor System. For example, a method ```IncreaseTargetTemperature```.

The Bridge Actor calls functions exposed by the ViewModel. These will update the ViewModel's properties. The actor does this only while handling messages.

### Bridge Interfaces

To avoid exposing the entire ViewModel to the Bridge Actor, an interface seems like a good idea. And an interface to abstract away the Bridge passed to the ViewModel would also help.

In short, we'll have two Interfaces. One implemented by the ViewModel, the other by the Bridge itself.

We can define these two interfaces:

* ```IThermostatView```
   * ```UpdateCurrentTemperature```
   * ```UpdateTargetTemperature```
* ```IThermostatBridge```
 * ```IncreaseTargetTemperature```
 * ```DecreaseTargetTemperature```

The Bridge Actor references ```IThermostatView``` while the ViewModel implements it.

The ViewModel references ```IThermostatBridge``` while the Bridge implements it.

## Implementation

### Views

The views, as discussed above, are simple:

```
// Implemented by the Bridge
public interface IThermostatBridge
{
    void IncreaseTargetTemperature();
    void DecreaseTargetTemperature();
}
```

```
// Implemented by the ViewModel (in the WPF project)
public interface IThermostatView
{
    void UpdateCurrentTemperature(double currentTemperature);
    void UpdateTargetTemperature(double targetTemperature);
}
```

### Bridge Actor

The Bridge Actor is small, just passing messages around. It does have two dependencies.

```
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
```

### Bridge

The bridge will implement the ```IThermostatBridge``` interface:

```
public class ThermostatBridge : IThermostatBridge
{
    private IActorRef _bridgeActor;

    private readonly IncreaseTargetTemperature increaseMessage = new IncreaseTargetTemperature(1);
    private readonly DecreaseTargetTemperature decreaseMessage = new DecreaseTargetTemperature(1);

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
```
### System Creation

I already had a seperate class to create the Actor System. For the excercise, I'll create the system when the WPF app starts. But this might not be the ideal place.

```
public partial class App : Application
{
    private static WpfAkkaIntegration.ThermostatSystem.ThermostatSystem _system = new WpfAkkaIntegration.ThermostatSystem.ThermostatSystem();

    public static WpfAkkaIntegration.ThermostatSystem.ThermostatSystem ThermostatSystem => _system;
}
```

In that class, there's a function ```CreateThermostatBridge``` which can create our Bridge. Here's the full ThermostatSystem class:

```
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
```

#### Synchronized Dispatcher

The code above creates the BridgeActor using the [synchronized dispatcher](http://getakka.net/docs/working-with-actors/Dispatchers#synchronizeddispatcher).

This makes that actor run on the UI-thread, so calling methods on the ViewModel should be safe.

From the [akka.net docs](http://getakka.net/docs/working-with-actors/Dispatchers#synchronizeddispatcher):

> You may use this dispatcher to create actors that update UIs in a reactive manner. An application that displays real-time updates of stock prices may have a dedicated actor to update the UI controls directly for example.

> **Note:** As a general rule, actors running in this dispatcher shouldn't do much work. Avoid doing any extra work that may be done by actors running in other pools.

### Bridge Creation

Finally we have everything to hook it all up in the ViewModel.

```
public MainViewModel()
{
    _bridge = App.ThermostatSystem.CreateThermostatBridge(this);

    IncreaseTargetTemperature = new RelayCommand(() => _bridge.IncreaseTargetTemperature());
    DecreaseTargetTemperature = new RelayCommand(() => _bridge.DecreaseTargetTemperature());
}
```

You can also see the implementation of the two commands the view can receive.

I do this in the constructor of the ViewModel for this excercise. In real life, I suppose dependency injection would be adviceable. But then the ViewModel would still need to assign itself to the Bridge afterwards.

## Result

Running the app gives me this WPF app that updates its view based on messages that occur in the Actor System.

Additionally, the view triggers changes to the system by sending it messages. It does this when it receives a command.

When you click a button in WPF, all it does is ask the bridge to increase the target temperature. The Actor System will then process that message. When it's done, the Actor System forwards a new message back to WPF so it can update the view.

This is all asynchronous, because of the actor system.

In the example, the current temperature only goes up. This is because I did not simulate an actual room that heats. I decided to just let the temperature increase at every measurement taken. Just to show the temperature is changing.

### Code

The example project is on github:

* [Finished](https://github.com/AaronLenoir/AkkaAndWpfExample/tree/bridged)
* [Before creating the bridge](https://github.com/AaronLenoir/AkkaAndWpfExample/tree/master)

## Conclusion

It's possible to loosely couple a WPF app with an Akka.NET Actor System. I used something I called a Bridge, [but there must be a better name](https://web.archive.org/web/20160420223239/http://martinfowler.com/bliki/TwoHardThings.html).

Something I don't like here is that the Bridge and Bridge Actor should be more generic. It mentions "Thermostat" everywhere.

Do you think this is a reasonable way to put a WPF front-end on an Akka.NET Actor System? Let me know in the comments, or on [twitter](https://www.twitter.com/lenoir_aaron) or something.
