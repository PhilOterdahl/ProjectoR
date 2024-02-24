using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProjectoR.Core.TypeResolvers;
using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student;

namespace ProjectoR.Examples.CustomSubscription.Data;

public class StudentRepository(ApplicationContext dbContext, IEventTypeResolver eventTypeResolver) : IStudentRepository
{
    public async Task<Student?> TryLoad(string id, CancellationToken cancellationToken)
    {
        var streamName = $"student-{id}";
        var eventRecords = await dbContext
            .Events
            .Where(@event => @event.StreamName == streamName)
            .OrderBy(@event => @event.Position)
            .ToArrayAsync(cancellationToken: cancellationToken);

        if (!eventRecords.Any())
            return null;

        var events = eventRecords
            .Select(record =>
                JsonSerializer.Deserialize(record.Data, eventTypeResolver.GetType(record.EventName))
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
            .Select(@event => new EventRecord
            {
                StreamName = streamName,
                Id = Guid.NewGuid(),
                EventName = eventTypeResolver.GetName(@events.GetType()),
                Data = JsonSerializer.SerializeToUtf8Bytes(@event)
            })
            .ToArray();

        var changesHasAppeared = await dbContext
            .Events
            .Where(record => record.StreamName == streamName)
            .AnyAsync(record => record.Position > student.CommitPosition, cancellationToken: cancellationToken);

        if (changesHasAppeared)
            throw new InvalidOperationException("Wrong commit position, changes has appeared");
        
        await dbContext.Events.AddRangeAsync(records);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        student.ClearUncommittedEvents();
    }
}