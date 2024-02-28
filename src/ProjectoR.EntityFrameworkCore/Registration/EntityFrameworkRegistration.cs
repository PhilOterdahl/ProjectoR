using ProjectoR.Core.Registration;

namespace ProjectoR.EntityFrameworkCore.Registration;

public static class EntityFrameworkRegistration
{
    public static IProjectoRConfigurator UseEntityFramework(
        this IProjectoRConfigurator configurator, 
        Action<IEntityFrameworkConfigurator> configure)
    {
        var entityFrameworkConfigurator = new EntityFrameworkConfigurator(configurator);
        configure(entityFrameworkConfigurator);
        return configurator;
    }
}