using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.Checkpointing;
using ProjectoR.EntityFrameworkCore.Checkpointing;

namespace ProjectoR.EntityFrameworkCore.Registration;

public interface IEntityFrameworkConfigurator
{
    IEntityFrameworkConfigurator UseEntityFrameworkCheckpointing<TDbContext>() where TDbContext : DbContext, ICheckpointingContext;
}

internal class EntityFrameworkConfigurator(IServiceCollection services) : IEntityFrameworkConfigurator
{
    public IEntityFrameworkConfigurator UseEntityFrameworkCheckpointing<TDbContext>() where TDbContext : DbContext, ICheckpointingContext
    {
        services
            .AddScoped<ICheckpointingContext>(provider => provider.GetRequiredService<TDbContext>())
            .AddScoped<ICheckpointRepository, EntityFrameworkCheckpointRepository>();
        
        return this;
    }
}