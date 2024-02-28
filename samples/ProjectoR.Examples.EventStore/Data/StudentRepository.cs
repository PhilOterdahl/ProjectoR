using System.Text.Json;
using EventStore.Client;
using ProjectoR.Core.TypeResolvers;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student;

namespace ProjectoR.Examples.EventStore.Data;

public class StudentRepository(EventStoreClient eventStoreClient, IEventTypeResolver eventTypeResolver) : IStudentRepository
{
    public async Task<Student?> TryLoad(string id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        var streamName = $"student-{id}";

        var response = eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            0,
            int.MaxValue,
            cancellationToken: cancellationToken);

        var status = await response.ReadState;

        if (status == ReadState.StreamNotFound)
            return null;

        var resolvedEvents = await response.ToArrayAsync(cancellationToken);
        var events = resolvedEvents
            .Select(record =>
                JsonSerializer.Deserialize(record.Event.Data.ToArray(), eventTypeResolver.GetType(record.Event.EventType))
            )
            .ToArray();

        var student = new Student();
        student.Load(events);

        return student;
    }

    public async Task<Student> Load(string id, CancellationToken cancellationToken)
    {
        var student = await TryLoad(id, cancellationToken);
        return student ?? throw new InvalidOperationException($"Student with Id: {id} was not found");
    }

    public async Task CommitEvents(Student student, CancellationToken cancellationToken)
    {
        var events = student.GetUncommittedEvents();
        var streamName = $"student-{student.Id}";
        
        var records = events
            .Select(@event => new EventData(
                Uuid.NewUuid(), 
                eventTypeResolver.GetName(@event.GetType()),
                JsonSerializer.SerializeToUtf8Bytes(@event))
            )
            .ToArray();
        
        if (student.CommitPosition == 0)
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.NoStream, 
                records,
                cancellationToken: cancellationToken
            );
        else
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamRevision.FromStreamPosition(new StreamPosition((ulong)student.CommitPosition - 1)), 
                records,
                cancellationToken: cancellationToken
            );
        
        student.ClearUncommittedEvents();
    }
}