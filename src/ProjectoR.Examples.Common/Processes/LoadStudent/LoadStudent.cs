using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student.Events;

namespace ProjectoR.Examples.Common.Processes.LoadStudent;

public static class LoadStudent
{
    public record Query(string Id);
    
    public class Handler(IStudentRepository repository)
    {
        public async Task<StudentState?> Handle(Query query, CancellationToken cancellationToken) => 
            await repository.TryLoad(query.Id, cancellationToken);
    }
}