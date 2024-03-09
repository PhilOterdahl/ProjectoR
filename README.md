ProjectoR
=======

![DEV](https://github.com/PhilOterdahl/ProjectoR/actions/workflows/main.yml/badge.svg?branch=development)
![CI](https://github.com/PhilOterdahl/ProjectoR/actions/workflows/main.yml/badge.svg?branch=main)

A simple way of writing projections in .NET

## Projector

### Simple projector example
Example of a simple projector using entity framework core.

A static property named ProjectionName is required with the name of the projection. 
This will be the name used when saving a checkpoint for the projection.

Methods inside the projector needs to have it's first parameter as the event it wants to react on.

Valid method names 

- Project
- Consume
- Consumes
- Handle
- Handles
- When

Valid return types

- Void
- Task
- ValueTask

Dependency injection is supported for project methods and all parameters except for the event and the cancellation token will be injected.

Static project methods are supported.


```cs
public class StudentProjector
{
    public static string ProjectionName => "Student";

    public static async Task Project(
        StudentWasEnrolled enrolled, 
        ISampleContext context, 
        CancellationToken cancellationToken)
    {
        context.Students.Add(new StudentProjection
        {
            Id = enrolled.Id,
            FirstName = enrolled.FirstName,
            LastName = enrolled.LastName,
            Address = new Address
            {
                CountryCode = enrolled.CountryCode,
                City = enrolled.City,
                Street = enrolled.Street,
                PostalCode = enrolled.PostalCode
            },
            ContactInformation = new ContactInformation
            {
                Email = enrolled.Email,
                Mobile = enrolled.Mobile
            }
        });

        await context.SaveChangesAsync(cancellationToken);
    }
    
    public static async Task Project(
        StudentRelocated relocated,
        ISampleContext context, 
        CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == relocated.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, relocated.NewAddress.City)
                    .SetProperty(projection => projection.Address.PostalCode, relocated.NewAddress.PostalCode)
                    .SetProperty(projection => projection.Address.Street, relocated.NewAddress.Street),
                cancellationToken
            );

    public static async Task Project(
        StudentChangedContactInformation changedContactInformation, 
        ISampleContext context,
        CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public static async Task Project(
        StudentGraduated studentGraduated, 
        ISampleContext context, 
        CancellationToken cancellationToken) =>
        await context
            .Students
            .Where(user => user.Id == studentGraduated.Id)
            .ExecuteDeleteAsync(cancellationToken);
}
```

### Pre processing 

Projectors have support for pre processing.

Pre processing will run before any project methods run and supports creating dependencies.

In this example it creates a transaction that will be disposed after the projector has finished.

The dependency created can be injected to any project methods.


Valid method names 

- PreProcess


```cs
public class StudentProjector
{
    public static string ProjectionName => "Student";

     public static async Task<IDbContextTransaction> PreProcess(
        ISampleContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task Project(
        StudentWasEnrolled enrolled,
        IDbContextTransaction transaction,
        ISampleContext context, 
        CancellationToken cancellationToken)
    {
        context.Students.Add(new StudentProjection
        {
            Id = enrolled.Id,
            FirstName = enrolled.FirstName,
            LastName = enrolled.LastName,
            Address = new Address
            {
                CountryCode = enrolled.CountryCode,
                City = enrolled.City,
                Street = enrolled.Street,
                PostalCode = enrolled.PostalCode
            },
            ContactInformation = new ContactInformation
            {
                Email = enrolled.Email,
                Mobile = enrolled.Mobile
            }
        });

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
```

### Post processing 

Projectors have support for post processing.

Post processing will run after all project methods have. 

In this example a transaction is created in the pre process stage and then later in the post processing stage is commited.
When a projector is processing events in batches this would be a stratgy for updating the projection with one database command for multiple events.


Valid method names 

- PostProcess

```cs
public class StudentProjector
{
    public static string ProjectionName => "Student";

    public static async Task<IDbContextTransaction> PreProcess(
        ISampleContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task PostProcess(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken) =>
        await transaction.CommitAsync(cancellationToken);
    
}
```

### Batching 

When registering a projector batching can be configured. 

If setting batch size to 100 it will process 100 events at a time. 
Pre and post processing will run once for 100 events. 

Batch timeout is the time it will wait before forcing a batch. 
If only 2 event has appeard for 500ms the projector will process 2 events instead of 100.

Batch size is by default set too 100 and batch timeout is set to 500ms.

```cs
builder
    .Services
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseSubscription<StudentProjector>(configure =>
                        {
                            configure.BatchingOptions.BatchSize = 100;
                            configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                        });
                }
            );
    });
```


### Checkpointing

#### Stratergies
There are 3 different checkpointing stratergies supported.

- EveryEvent
- Interval
- AfterBatch

The default stratergy is set to save a checkpoint after every event but can be changed when registering a projector.

```cs
builder
    .Services
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseSubscription<StudentProjector>(configure =>
                        {
                            configure.CheckpointingOptions.CheckpointAfterBatch();
                        });
                }
            );
    });
```

####
Storage

There are multiple different storages supported for saving checkpoints.

- EventStoreDB
- EntityFrameworkCore
- InMemory

Check EventStoreDB and EntityFrameworkCore section for how to use them as storage for checkpoints.

Example for using inmemory checkpoint should probably only be used for testing.
```cs
builder
    .Services
    .AddProjectoR(configurator =>
    {
         configurator.UseInMemoryCheckpointing();
    });
```



### Event type resolver

A event type resolver is needed to convert events read from the database to a c# class.
The name of the event is stored in the used event store or streaming provider and needs to be mapped to a c# class. 
There are heaps of stratergies on how to name your event when storing them depending on versioning and much more.

There are some built in but in most case a custom one should be used. 

These are the supported event type resolvers.

- Namespace
- ClassName
- Custom

Example of a custom event type resolver that loads all event for a specific namespace.
This event type resolver uses class name.

```cs
public class CustomTypeResolver : IEventTypeResolver
{
    private readonly IReadOnlyDictionary<string, Type> _eventTypes;

    public CustomTypeResolver()
    {
        _eventTypes = typeof(StudentGraduated)
            .Assembly
            .GetTypes()
            .Where(type => type.Namespace == typeof(StudentGraduated).Namespace)
            .ToDictionary(GetName, type => type);
    }

    public Type GetType(string eventName) =>
        _eventTypes.TryGetValue(eventName, out var type)
            ? type
            : throw new InvalidOperationException($"Type for event with name: {eventName} was not found");

    public string GetName(Type eventType) => eventType.Name;
}

builder
    .Services
    .AddProjectoR(configurator =>
    {
        configurator
            .UseEventStore(
                builder.Configuration.GetConnectionString("EventStoreDB"),
                eventStoreConfigurator =>
                {
                    eventStoreConfigurator
                        .UseSubscription<StudentProjector>(configure =>
                        {
                            configure.SerializationOptions.UseCustomEventTypeResolver<CustomTypeResolver>();
                        });
                }
            );
    });
```


### EventStoreDB

To use ProjectoR with EventStoreDB You should install [ProjectoR.EvenStore](https://www.nuget.org/packages/ProjectoR.EventStore):


### EntityFrameworkCore

### CustomSubscription

ProjectoR supports custom subscriptions this way if there is an event store or event streaming provider that is not supported a custom subscription can be written to integrate with it. 

A custom subscription needs to return a IAsyncEnumerable or IEnumerable of EventData. 

The parameters provided 
 - serviceProvider so a scope can be created and needed services can be injected. 
 - eventNames a list of the events that the projector this subscription is used for is consuming, this is need for filtering out events.
 - checkPoint the last checkpoint stored for the projector.
 - cancellationToken 


Valid method names

- Subscribe

Example for a sql subscription that is using polling to the check against an table of events.

```cs
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

```

Registering a projector that uses a custom subscription

```cs

builder
    .Services
    .AddProjectoR(configurator =>
    {
         configurator
            .UseCustomSubscription<EntityFrameworkCoreSubscription, StudentProjector>(configure =>
            {
                configure.BatchingOptions.BatchSize = 100;
                configure.BatchingOptions.BatchTimeout = TimeSpan.FromMilliseconds(100);
                configure.CheckpointingOptions.CheckpointAfterBatch();
            })
    });

```







