using Microsoft.EntityFrameworkCore;
using ProjectoR.Examples.EventStoreDB.Data;

namespace ProjectoR.Examples.EventStoreDB;

public record UserEnrolled(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Mobile,
    string Country,
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

public record UserQuit(Guid Id, string Country);


public record UserChangedEmail(Guid Id, string Email);


public class UserProjector
{
    public static string ProjectionName => "User";
    
    public static async Task Handle(
        UserEnrolled enrolled, 
        UserContext context, 
        CancellationToken cancellationToken)
    {
        context.UsersProjections.Add(new UserProjection
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
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public void When(UserChangedEmail emailChanged, UserContext context, CancellationToken cancellationToken) =>
        context
            .UsersProjections
            .Where(user => user.Id == emailChanged.Id)
            .ExecuteUpdate(calls => calls.SetProperty(projection => projection.ContactInformation.Email, emailChanged.Email));
    
    public Task Consume(UserChangedEmail emailChanged, UserContext context, CancellationToken cancellationToken) =>
        context
            .UsersProjections
            .Where(user => user.Id == emailChanged.Id)
            .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.ContactInformation.Email, emailChanged.Email));
    
    public async Task When(UserMoved moved, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == moved.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, moved.City)
                    .SetProperty(projection => projection.Address.PostalCode, moved.PostalCode)
                    .SetProperty(projection => projection.Address.Street, moved.Street),
                cancellationToken
            );

    public async Task When(UserChangedContactInformation changedContactInformation, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public async Task When(UserQuit userQuit, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == userQuit.Id)
            .ExecuteDeleteAsync(cancellationToken);
}