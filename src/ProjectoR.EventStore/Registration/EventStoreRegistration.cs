using ProjectoR.Core.Registration;

namespace ProjectoR.EventStore.Registration;

public static class EventStoreRegistration
{
    public static IProjectoRConfigurator UseEventStore(
        this IProjectoRConfigurator configurator, 
        string connectionString,
        Action<IEventStoreConfigurator> configure)
    {
        var eventStoreConfigurator = new EventStoreConfigurator(configurator, connectionString);
        configure(eventStoreConfigurator);
        return configurator;
    }
}