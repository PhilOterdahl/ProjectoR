using ProjectoR.Examples.Common.Data;

namespace ProjectoR.Examples.Common.MoveUser;

public static class MoveUser
{
    public record Command(
        string Id, 
        string CountryCode,
        string City,
        string PostalCode,
        string Street);
    
    public class Handler(IUserRepository repository)
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var (id, countryCode, city, postalCode, street) = command;
            var user = await repository.Load(id, cancellationToken);

            user.Move(countryCode, city, postalCode, street);

            await repository.CommitEvents(user, cancellationToken);
        }
    }
}