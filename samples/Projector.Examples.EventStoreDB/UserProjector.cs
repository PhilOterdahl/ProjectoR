using Microsoft.EntityFrameworkCore;
using ProjectoR.Examples.EventStoreDB.Data;

namespace ProjectoR.Examples.EventStoreDB;

public class User
{
    public record Enrolled(
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

    public record Moved(
        Guid Id,
        string City,
        string PostalCode,
        string Street
    );

    public record ChangedContactInformation(
        Guid Id,
        string Mobile,
        string Email
    );
    
    public record Quit(Guid Id, string Country);


    public record ChangedEmail(Guid Id, string Email);
}


public class UserProjector
{
    public static string ProjectionName => "User";
    
    public static async Task Handle(
        User.Enrolled enrolled, 
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
    
    public void When(User.ChangedEmail emailChanged, UserContext context, CancellationToken cancellationToken) =>
        context
            .UsersProjections
            .Where(user => user.Id == emailChanged.Id)
            .ExecuteUpdate(calls => calls.SetProperty(projection => projection.ContactInformation.Email, emailChanged.Email));
    
    public async Task When(User.Moved moved, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == moved.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, moved.City)
                    .SetProperty(projection => projection.Address.PostalCode, moved.PostalCode)
                    .SetProperty(projection => projection.Address.Street, moved.Street),
                cancellationToken
            );

    public async Task When(User.ChangedContactInformation changedContactInformation, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public async Task When(User.Quit userQuit, UserContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == userQuit.Id)
            .ExecuteDeleteAsync(cancellationToken);
}