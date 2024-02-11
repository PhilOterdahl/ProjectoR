using ProjectoR.Examples.Common.Data;
using ProjectoR.Examples.Common.Domain;
using ProjectoR.Examples.Common.Domain.User;

namespace ProjectoR.Examples.Common.EnrollUser;

public static class EnrollUser
{
    public record Command(
        string Id,  
        string FirstName,
        string LastName,
        string Email,
        string Mobile,
        string CountryCode,
        string City,
        string PostalCode,
        string Street
    );
    
    public class Handler(IUserRepository repository)
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var (
                id, 
                firstName, 
                lastName, 
                email, 
                mobile, 
                countryCode, 
                city, 
                postalCode, 
                street) = command;

            var user = await repository.TryLoad(id, cancellationToken);

            if (user is not null)
                throw new InvalidOperationException($"User with Id: {id} already exists");

            user = User.Enroll(id, firstName, lastName, email, mobile, countryCode, city, postalCode, street);

            await repository.CommitEvents(user, cancellationToken);
        }
    }
}