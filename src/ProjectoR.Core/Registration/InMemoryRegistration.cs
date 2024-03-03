namespace ProjectoR.Core.Registration;

public static class InMemoryRegistration
{
    public static IProjectoRConfigurator UseInMemoryCheckpointing(this IProjectoRConfigurator configurator)
    {
        var _ = new InMemoryConfigurator(configurator);
        return configurator;
    }
}