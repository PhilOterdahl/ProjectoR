using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain.Student;

namespace ProjectoR.Examples.Common.Processes.EnrollStudent;

public static class EnrollStudent
{
    public record Command(
        string FirstName,
        string LastName,
        string Email,
        string Mobile,
        string CountryCode,
        string City,
        string PostalCode,
        string Street
    );
    
    public class Handler(IStudentRepository repository)
    {
        public async Task<string> Handle(Command command, CancellationToken cancellationToken)
        {
            var (
                firstName, 
                lastName, 
                email, 
                mobile, 
                countryCode, 
                city, 
                postalCode, 
                street) = command;

            var student = Student.Enroll(firstName, lastName, email, mobile, countryCode, city, postalCode, street);

            await repository.CommitEvents(student, cancellationToken);

            return student.Id;
        }
    }
}