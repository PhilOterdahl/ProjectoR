using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.Common.Processes.RelocateStudent;

public static class RelocateStudent
{
    public record Command(
        string Id, 
        string CountryCode,
        string City,
        string PostalCode,
        string Street);
    
    public class Handler(IStudentRepository repository)
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var (id, countryCode, city, postalCode, street) = command;
            var student = await repository.Load(id, cancellationToken);

            student.Relocate(countryCode, city, postalCode, street);

            await repository.CommitEvents(student, cancellationToken);
        }
    }
}