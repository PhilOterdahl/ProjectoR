using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Projector.Examples.EventStoreDB;

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
            new UserEnrolled(
                pepeId,
                "Pepe",
                "Silva",
                "Pepe.Silva@hotmail.com",
                "0732603999",
                "Philadelphia",
                "19019",
                "Apple Blossom Way 13"
            ),
            new UserEnrolled(
                charlieId,
                "Charlie",
                "Kelly",
                "Charlie.Kelly@hotmail.com",
                "0732623429",
                "Philadelphia",
                "19019",
                "Apple Blossom Way 13"
            ),
            new UserEnrolled(
                dennisId,
                "Dennis",
                "Reynlods",
                "Dennis.Reynlods@hotmail.com",
                "073260312399",
                "Philadelphia",
                "19021",
                "Audubon Plaza 18"
            ),
            new UserMoved(
                dennisId,
                "Philadelphia",
                "19014",
                "Bellevue Steet 15"
            ),
            new UserChangedContactInformation(
                pepeId,
                "04565567567",
                "Pepe.Sliva@hotmail.com"
            )
        };

        var data = events.Select(@event => new EventData(
            Uuid.NewUuid(),
            @event.GetType().FullName,
            JsonSerializer.SerializeToUtf8Bytes(@event).ToArray()
        ));
        
        await eventStoreClient.AppendToStreamAsync("test-stream", StreamState.Any, data, cancellationToken: stoppingToken);
    }
}