using ProjectoR.Core.Registration;

namespace ProjectoR.EventStoreDb.Registration;

public static class EventStoreDBRegistration
{
    public static void UseEventStoreDB(
        this ProjectoRConfigurator configurator, 
        string connectionString,
        Action<IEventStoreDbConfigurator> configure)
    {
        var eventStoreDBConfigurator = new EventStoreDbConfigurator(configurator.Services, connectionString);
        configure(eventStoreDBConfigurator);
    }
}