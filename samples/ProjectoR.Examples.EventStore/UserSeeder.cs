using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectoR.Core.EventNameFormatters;
using ProjectoR.Core.Projector;
using ProjectoR.Core.TypeResolvers;

namespace ProjectoR.Examples.EventStore;

public class UserSeeder : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public UserSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var eventStoreClient = provider.GetRequiredService<EventStoreClient>();
        
        var pepeId = Guid.NewGuid();
        var charlieId = Guid.NewGuid();
        var dennisId = Guid.NewGuid();

        var events = new object[]
        {
            new User.Enrolled(
                pepeId,
                "Pepe",
                "Silva",
                "Pepe.Silva@hotmail.com",
                "0732603999",
                "USA",
                "Philadelphia",
                "19019",
                "Apple Blossom Way 13"
            ),
            new User.Enrolled(
                charlieId,
                "Charlie",
                "Kelly",
                "Charlie.Kelly@hotmail.com",
                "0732623429",
                "USA",
                "Philadelphia",
                "19019",
                "Apple Blossom Way 13"
            ),
            new User.Enrolled(
                dennisId,
                "Dennis",
                "Reynlods",
                "Dennis.Reynlods@hotmail.com",
                "073260312399",
                "USA",
                "Philadelphia",
                "19021",
                "Audubon Plaza 18"
            ),
            new User.Moved(
                dennisId,
                "Philadelphia",
                "19014",
                "Bellevue Steet 15"
            ),
            new User.ChangedContactInformation(
                pepeId,
                "04565567567",
                "Pepe.Sliva@hotmail.com"
            ),
            new User.Quit(
                dennisId,
               "USA"
            ),
        };
        
        var options = provider.GetRequiredKeyedService<ProjectorOptions>("User");
        var eventTypeResolver = provider.GetRequiredService<EventTypeResolverProvider>()
            .GetEventTypeResolver(
                "User",
                options.SerializationOptions.EventTypeResolver,
                options.SerializationOptions.Casing,
                options.SerializationOptions.CustomEventTypeResolverType,
                events.Select(@event => @event.GetType()).Distinct().ToArray()
            );
        
        var data = events.Select(@event => new EventData(
            Uuid.NewUuid(),
            eventTypeResolver.GetName(@event.GetType()),
            JsonSerializer.SerializeToUtf8Bytes(@event).ToArray()
        ));
        
        await eventStoreClient.AppendToStreamAsync("test-stream", StreamState.Any, data, cancellationToken: stoppingToken);
    }
}