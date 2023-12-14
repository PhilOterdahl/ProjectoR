using Microsoft.EntityFrameworkCore;
using Projector.Core.Projector;
using Projector.Examples.EventStoreDB.Data;

namespace Projector.Examples.EventStoreDB;

public record UserEnrolled(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Mobile,
    string City,
    string PostalCode,
    string Street
);

public record UserMoved(
    Guid Id,
    string City,
    string PostalCode,
    string Street
);

public record UserChangedContactInformation(
    Guid Id,
    string Mobile,
    string Email
);

public record UserQuit(Guid Id);

public class UserProjector : Projector<UserContext>
{
    public override string ProjectionName => "User";

    public UserProjector(UserContext connection, IServiceProvider serviceProvider) : base(connection, serviceProvider)
    {
        When<UserEnrolled>(async (context, enrolled, cancellation) =>
        {
            context.Users.Add(new UserProjection
            {
                Id = enrolled.Id,
                FirstName = enrolled.FirstName,
                LastName = enrolled.LastName,
                Address = new Address
                {
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
            await context.SaveChangesAsync(cancellation);
        });
        
        When<UserMoved>(async (context, moved, cancellationToken) =>
        {
            await context
                .Users
                .Where(user => user.Id == moved.Id)
                .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, moved.City)
                    .SetProperty(projection => projection.Address.PostalCode, moved.PostalCode)
                    .SetProperty(projection => projection.Address.Street, moved.Street),
                    cancellationToken
                );

        });
        
        When<UserChangedContactInformation>(async (context, changedContactInformation, cancellationToken) =>
        {
            await context
                .Users
                .Where(user => user.Id == changedContactInformation.Id)
                .ExecuteUpdateAsync(calls => calls
                        .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                        .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                    cancellationToken
                );
        });
        
        When<UserQuit>(async (context, changedContactInformation, cancellationToken) =>
        {
            await context
                .Users
                .Where(user => user.Id == changedContactInformation.Id)
                .ExecuteDeleteAsync(cancellationToken);
        });
    }
}