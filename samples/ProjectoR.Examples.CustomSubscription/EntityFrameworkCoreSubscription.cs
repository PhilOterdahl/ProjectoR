using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ProjectoR.Core;
using ProjectoR.Examples.CustomSubscription.Data;

namespace ProjectoR.Examples.CustomSubscription;

public class EntityFrameworkCoreSubscription
{
    private const int BatchSize = 100;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    
    public static async IAsyncEnumerable<EventData> Subscribe(
        IServiceProvider serviceProvider,
        string[] eventNames,
        long? checkpoint,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);
        var moreEventsExists = false;
        var position = checkpoint ?? 0;
        
        while (moreEventsExists || await timer.WaitForNextTickAsync(cancellationToken))
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var eventContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            
            var lastEventPosition = position;
            var lastReadPosition = position;
            var events = eventContext
                .Events
                .Where(@event => eventNames.Contains(@event.EventName))
                .Where(@event => @event.Position > lastReadPosition)
                .Take(BatchSize)
                .Select(@event => new EventData(@event.EventName, @event.Data, @event.Position));
            
            await foreach (var @event in events.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                yield return @event;
                lastEventPosition = @event.Position;
                position = @event.Position;
            }
            
            moreEventsExists = await eventContext
                .Events
                .Where(@event => eventNames.Contains(@event.EventName))
                .Where(@event => @event.Position > lastEventPosition)
                .AnyAsync(cancellationToken: cancellationToken);
        }
    }
}