using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.Common.Processes.GraduateStudent;

public static class GraduateStudent
{
    public record Command(string Id);
    
    public class Handler(IStudentRepository repository)
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var student = await repository.Load(command.Id, cancellationToken);

            student.Graduate();

            await repository.CommitEvents(student, cancellationToken);
        }
    }
}