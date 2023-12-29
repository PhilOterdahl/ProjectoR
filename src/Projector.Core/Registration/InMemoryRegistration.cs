namespace ProjectoR.Core.Registration;

public static class InMemoryRegistration
{
    public static ProjectoRConfigurator UseInMemory(
        this ProjectoRConfigurator configurator,
        Action<IInMemoryConfigurator> configure)
    {
        var inMemoryConfigurator = new InMemoryConfigurator(configurator.Services);
        configure(inMemoryConfigurator);
        return configurator;
    }
}