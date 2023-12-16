using Microsoft.Extensions.DependencyInjection;

namespace ProjectoR.Core.Registration;

public class ProjectoRConfigurator(IServiceCollection services)
{
    public IServiceCollection Services { get; private set; } = services;
}