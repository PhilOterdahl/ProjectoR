using System.Text.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core.TypeResolvers;
using ProjectoR.Examples.Common;

namespace ProjectoR.Examples.EventStore;

public static class UserSeeder
{
    public static async Task Seed(
        int seedAmount,
        IServiceProvider serviceProvider, 
        CancellationToken stoppingToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var eventStoreClient = provider.GetRequiredService<EventStoreClient>();
        var events = Enumerable
            .Range(0, seedAmount)
            .SelectMany(_ =>
            {
                var pepeId = Guid.NewGuid();
                var charlieId = Guid.NewGuid();
                var dennisId = Guid.NewGuid();
                var philId = Guid.NewGuid();

                return new object[]
                {
                    new User.Enrolled(
                        pepeId,
                        "Pepe",
                        "Silva",
                        "Pepe.Silva@hotmail.com",
                        "0732603999",
                        "US",
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
                        "US",
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
                        "US",
                        "Philadelphia",
                        "19021",
                        "Audubon Plaza 18"
                    ),
                    new User.Enrolled(
                        philId,
                        "Phil",
                        "Dahlen",
                        "Phil.Dahlen@hotmail.com",
                        "07326031234299",
                        "SE",
                        "Stockholm",
                        "17053",
                        "Bästegatan 24"
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
            });
        
            var eventTypeResolver = provider.GetRequiredKeyedService<IEventTypeResolver>("AmountOfUsersPerCity");
            var chunks = events
                .Select(@event => new EventData(
                    Uuid.NewUuid(),
                    eventTypeResolver.GetName(@event.GetType()),
                    JsonSerializer.SerializeToUtf8Bytes(@event).ToArray()
                ))
                .Chunk(2000);

            await Parallel.ForEachAsync(
                chunks, 
                stoppingToken,
                async (data, token) => await eventStoreClient.AppendToStreamAsync("test-stream", StreamState.Any, data, cancellationToken: stoppingToken)
            );
    }
}