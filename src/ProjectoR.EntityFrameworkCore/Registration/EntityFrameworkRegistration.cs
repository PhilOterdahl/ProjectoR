using ProjectoR.Core.Registration;

namespace ProjectoR.EntityFrameworkCore.Registration;

public static class EntityFrameworkRegistration
{
    public static ProjectoRConfigurator UseEntityFramework(
        this ProjectoRConfigurator configurator, 
        Action<IEntityFrameworkConfigurator> configure)
    {
        var entityFrameworkConfigurator = new EntityFrameworkConfigurator(configurator.Services);
        configure(entityFrameworkConfigurator);
        return configurator;
    }
}