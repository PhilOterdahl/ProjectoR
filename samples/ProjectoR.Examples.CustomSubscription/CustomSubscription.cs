using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectoR.Core;
using ProjectoR.Examples.CustomSubscription.Data;

namespace ProjectoR.Examples.CustomSubscription;

public class CustomSubscription
{
    private const int BatchSize = 100;
    private static readonly TimeSpan PoolingInterval = TimeSpan.FromSeconds(1);
    
    public static async IAsyncEnumerable<EventData> Subscribe(
        IServiceProvider serviceProvider,
        string[] eventNames,
        long? checkpoint,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(PoolingInterval);
        var moreEventsExists = false;
        
        while (moreEventsExists || await timer.WaitForNextTickAsync(cancellationToken))
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var eventContext = scope.ServiceProvider.GetRequiredService<CustomSubscriptionContext>();

            var position = (int)(checkpoint ?? 0);
            var lastEventPosition = 0;
            var events = eventContext
                .Events
                .Where(@event => eventNames.Contains(@event.Name))
                .Skip(position)
                .Take(BatchSize)
                .Select(@event => new EventData(@event.Name, @event.Data, @event.Position));
            
            await foreach (var @event in events.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return @event;
                lastEventPosition = (int)@event.Position;
            }
            
            moreEventsExists = await eventContext
                .Events
                .Where(@event => eventNames.Contains(@event.Name))
                .Where(@event => @event.Position > @lastEventPosition)
                .AnyAsync(cancellationToken: cancellationToken);
        }
    }
}