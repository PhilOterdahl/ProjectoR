using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.Common.Processes.ChangeStudentContactInformation;

public static class ChangeStudentContactInformation
{
    public record Command(string Id, string Mobile, string Email);
    
    public class Handler(IStudentRepository repository)
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var (id, mobile, email) = command;
            var student = await repository.Load(id, cancellationToken);

            student.ChangeContactInformation(mobile, email);

            await repository.CommitEvents(student, cancellationToken);
        }
    }
}