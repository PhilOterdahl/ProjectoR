using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectoR.Examples.EventStore;

namespace ProjectoR.Examples.Common;

public class User
{
    public record Enrolled(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Mobile,
        string CountryCode,
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
    
    public static async Task<IDbContextTransaction> PreProcess(
        ApplicationContext context,
        CancellationToken cancellationToken) =>
        await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted, cancellationToken);

    public static async Task PostProcess(
        ApplicationContext context,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public static async Task Project(
        User.Enrolled enrolled, 
        IDbContextTransaction transaction,
        ApplicationContext context, 
        CancellationToken cancellationToken)
    {
        context.UsersProjections.Add(new UserProjection
        {
            Id = enrolled.Id,
            FirstName = enrolled.FirstName,
            LastName = enrolled.LastName,
            Address = new Address
            {
                CountryCode = enrolled.CountryCode,
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
    
    public async Task Project(User.ChangedEmail emailChanged, ApplicationContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == emailChanged.Id)
            .ExecuteUpdateAsync(calls => calls.SetProperty(projection => projection.ContactInformation.Email, emailChanged.Email), cancellationToken: cancellationToken);
    
    public async Task Project(User.Moved moved, ApplicationContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == moved.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.Address.City, moved.City)
                    .SetProperty(projection => projection.Address.PostalCode, moved.PostalCode)
                    .SetProperty(projection => projection.Address.Street, moved.Street),
                cancellationToken
            );

    public async Task Project(User.ChangedContactInformation changedContactInformation, ApplicationContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == changedContactInformation.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(projection => projection.ContactInformation.Email, changedContactInformation.Email)
                    .SetProperty(projection => projection.ContactInformation.Mobile, changedContactInformation.Mobile),
                cancellationToken
            );

    public async Task Project(User.Quit userQuit, ApplicationContext context, CancellationToken cancellationToken) =>
        await context
            .UsersProjections
            .Where(user => user.Id == userQuit.Id)
            .ExecuteDeleteAsync(cancellationToken);
}