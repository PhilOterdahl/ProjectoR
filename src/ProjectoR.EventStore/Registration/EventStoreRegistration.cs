using ProjectoR.Core.Registration;

namespace Projector.EventStore.Registration;

public static class EventStoreRegistration
{
    public static ProjectoRConfigurator UseEventStore(
        this ProjectoRConfigurator configurator, 
        string connectionString,
        Action<IEventStoreConfigurator> configure)
    {
        var eventStoreConfigurator = new EventStoreConfigurator(configurator, connectionString);
        configure(eventStoreConfigurator);
        return configurator;
    }
}